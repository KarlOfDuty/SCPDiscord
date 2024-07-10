using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;
using PluginAPI.Core;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SCPDiscord
{
    public static class Utilities
	{
		public class FileWatcher : IDisposable
		{
			private readonly FileSystemWatcher watcher;
			private readonly Action action;
			public FileWatcher(string dirPath, string fileName, Action func)
			{
				action = func;
				watcher = new FileSystemWatcher(dirPath, fileName);
				watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
				watcher.EnableRaisingEvents = true;
				watcher.Changed += OnChanged;
				watcher.Created += OnChanged;
			}

			private void OnChanged(object sender, FileSystemEventArgs e)
			{
				action();
			}

			private void Dispose(bool disposing)
			{
				if (disposing)
				{
					watcher?.Dispose();
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

        public static string SecondsToCompoundTime(long seconds)
        {
            if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (seconds == 0) return "0 sec";

            TimeSpan span = TimeSpan.FromSeconds(seconds);
            int[] parts = { span.Days / 365, span.Days % 365 / 31,  span.Days % 365 % 31, span.Hours, span.Minutes, span.Seconds };
            string[] units = { " year", " month", " day", " hour", " minute", " second" };

            return string.Join(", ",
                from index in Enumerable.Range(0, units.Length)
                where parts[index] > 0
                select parts[index] + (parts[index] == 1 ? units[index] : units[index] + "s"));
        }

		public static string TicksToCompoundTime(long ticks)
		{
			return SecondsToCompoundTime(ticks / TimeSpan.TicksPerSecond);
		}

		public static bool TryGetPlayerName(string userID, out string name)
		{
			foreach (Player player in Player.GetPlayers<Player>())
			{
				if (userID.Contains(player.GetParsedUserID()))
				{
					name = player.Nickname;
					return true;
				}
			}

			name = "";
			return false;
		}

		public static bool IsPossibleSteamID(string steamID, out ulong id)
		{
			id = 0;
			return steamID.Length >= 17 && ulong.TryParse(steamID.Replace("@steam", ""), out id);
		}

		public static LinkedList<string> ParseListIntoMessages(List<string> listItems)
		{
			LinkedList<string> messages = new LinkedList<string>();
			foreach (string listItem in listItems)
			{
				if (messages.Last?.Value?.Length + listItem?.Length < 2048)
				{
					messages.Last.Value += listItem + "\n";
				}
				else
				{
					messages.AddLast(listItem?.Trim());
				}
			}

			return messages;
		}

		public static DateTime ParseCompoundDuration(string duration, ref long durationSeconds)
        {
            TimeSpan timeSpanDuration = TimeSpan.Zero;
            MatchCollection matches = Regex.Matches(duration, @"(\d+)([smhdwMy])");

			// If no matches are found, try to parse the duration as a single number of minutes
			if (matches.Count == 0)
			{
				if (long.TryParse(duration, out long minutes))
				{
					durationSeconds = minutes * 60;
					return DateTime.UtcNow.AddMinutes(minutes);
				}
				else
				{
					durationSeconds = 0;
					return DateTime.MinValue;
				}
			}

			foreach (Match match in matches)
			{
				int amount = int.Parse(match.Groups[1].Value);
				char unit = match.Groups[2].Value[0];

				switch (unit)
				{
					case 's':
						timeSpanDuration += TimeSpan.FromSeconds(amount);
						break;
					case 'm':
						timeSpanDuration += TimeSpan.FromMinutes(amount);
						break;
					case 'h':
						timeSpanDuration += TimeSpan.FromHours(amount);
						break;
					case 'd':
						timeSpanDuration += TimeSpan.FromDays(amount);
						break;
					case 'w':
						timeSpanDuration += TimeSpan.FromDays(amount * 7);
						break;
					case 'M':
						timeSpanDuration += TimeSpan.FromDays(amount * 31);
						break;
					case 'y':
						timeSpanDuration += TimeSpan.FromDays(amount * 365);
						break;
					default:
						durationSeconds = 0;
						return DateTime.MinValue;
				}
			}

            durationSeconds = (long)timeSpanDuration.TotalSeconds;
            return DateTime.UtcNow.Add(timeSpanDuration);
        }

		public static Interface.BotActivity.Types.Activity ParseBotActivity(string activity)
		{
			if (!Enum.TryParse(activity, true, out Interface.BotActivity.Types.Activity result))
			{
				Logger.Warn("Bot activity type '" + activity + "' invalid, using 'Playing' instead.");
				return Interface.BotActivity.Types.Activity.Playing;
			}

			return result;
		}

		public static Interface.BotActivity.Types.Status ParseBotStatus(string status)
		{
			if (!Enum.TryParse(status, true, out Interface.BotActivity.Types.Status result))
			{
				Logger.Warn("Bot status type '" + status + "' invalid, using 'Online' instead.");
				return Interface.BotActivity.Types.Status.Online;
			}

			return result;
		}

		public static bool TryGetSteamName(string userID, out string steamName)
		{
			steamName = null;
			if (!IsPossibleSteamID(userID, out ulong _))
			{
				return false;
			}

			userID = userID.Replace("@steam", "");

			HttpWebResponse response = null;
			ServicePointManager.ServerCertificateValidationCallback = SSLValidation;
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/profiles/" + userID + "?xml=1");
				request.Method = "GET";

				response = (HttpWebResponse)request.GetResponse();
				Stream responseStream = response.GetResponseStream();

				if (responseStream == null)
				{
					return false;
				}

				string xmlResponse = new StreamReader(responseStream).ReadToEnd();

				XmlDocument xml = new XmlDocument
				{
					XmlResolver = null
				};
				xml.LoadXml(xmlResponse);
				steamName = xml.DocumentElement?.SelectSingleNode("/profile/steamID")?.InnerText;

				response.Close();
				return !string.IsNullOrWhiteSpace(steamName);
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					Logger.Error("Steam profile connection error: " + ((HttpWebResponse)e.Response).StatusCode);
				}
				else
				{
					Logger.Error("Steam profile connection error: " + e.Status);
				}
			}
			finally
			{
				response?.Close();
			}
			return false;
		}

		private static bool SSLValidation(object _, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				return true;
			}

			// If there are errors in the certificate chain, look at each error to determine the cause.
			foreach (X509ChainStatus certChainStatus in chain.ChainStatus)
			{
				if (certChainStatus.Status == X509ChainStatusFlags.RevocationStatusUnknown)
				{
					continue;
				}

				chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
				chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
				chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
				chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

				// chain.Build returns false if the certificate chain is invalid
				if (!chain.Build((X509Certificate2)certificate))
				{
					return false;
				}
			}
			return true;
		}

		public static string ReadManifestData(string embeddedFileName)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string resourceName;
			try
			{
				resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName,StringComparison.CurrentCultureIgnoreCase));
			}
			catch(Exception e)
			{
				throw new InvalidOperationException("Could not load file (" + embeddedFileName + ") from manifest: " + e);
			}

			Stream stream;
			try
			{
				stream = assembly.GetManifestResourceStream(resourceName);
			}
			catch(Exception e)
			{
				throw new InvalidOperationException("Could not load file (" + embeddedFileName + ") from manifest: " + e);
			}

			if (stream == null)
			{
				throw new InvalidOperationException("Could not load manifest resource stream.");
			}

			StreamReader reader = new StreamReader(stream);
			string fileContents = reader.ReadToEnd();

			reader.Dispose();
			stream.Dispose();

			return fileContents;
		}

		/// <summary>
		/// This function makes me want to die too, don't worry.
		/// Parses a yaml file into a yaml object, parses the yaml object into a json string, parses the json string into a json object
		/// </summary>
		public static JObject LoadYamlFile(string fileName)
		{
			// Reads file contents into FileStream
			FileStream stream = File.OpenRead(fileName);

			// Converts the FileStream into a YAML Dictionary object
			IDeserializer deserializer = new DeserializerBuilder().Build();
			object yamlObject = deserializer.Deserialize(new StreamReader(stream));

			// Converts the YAML Dictionary into JSON String
			ISerializer serializer = new SerializerBuilder()
				.JsonCompatible()
				.Build();

			if (yamlObject == null)
			{
				throw new YamlException("Could not read YAML file: '" + fileName + "'. Is it a valid YAML file?");
			}

			string jsonString = serializer.Serialize(yamlObject);
			return JObject.Parse(jsonString);
		}
	}
}
