using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using PluginAPI.Core;

namespace SCPDiscord
{
    public static class Utilities
	{
		public class FileWatcher : IDisposable
		{
			private FileSystemWatcher watcher;
			private Action action;
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

		public static string EscapeDiscordFormatting(string input)
		{
			input = input.Replace("`", "\\`");
			input = input.Replace("*", "\\*");
			input = input.Replace("_", "\\_");
			input = input.Replace("~", "\\~");
			return input;
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

		public static DateTime ParseCompoundDuration(string duration, ref string humanReadableDuration, ref long durationSeconds)
		{
			//Check if the amount is a number
			if (!int.TryParse(new string(duration.Where(char.IsDigit).ToArray()), out int amount))
			{
				return DateTime.MinValue;
			}

			char unit = duration.Where(char.IsLetter).ToArray()[0];
			TimeSpan timeSpanDuration = new TimeSpan();

			// Parse time into a TimeSpan duration and string
			if (unit == 's')
			{
				humanReadableDuration = amount + " second";
				timeSpanDuration = new TimeSpan(0, 0, 0, amount);
			}
			else if (unit == 'm')
			{
				humanReadableDuration = amount + " minute";
				timeSpanDuration = new TimeSpan(0, 0, amount, 0);
			}
			else if (unit == 'h')
			{
				humanReadableDuration = amount + " hour";
				timeSpanDuration = new TimeSpan(0, amount, 0, 0);
			}
			else if (unit == 'd')
			{
				humanReadableDuration = amount + " day";
				timeSpanDuration = new TimeSpan(amount, 0, 0, 0);
			}
			else if (unit == 'w')
			{
				humanReadableDuration = amount + " week";
				timeSpanDuration = new TimeSpan(7 * amount, 0, 0, 0);
			}
			else if (unit == 'M')
			{
				humanReadableDuration = amount + " month";
				timeSpanDuration = new TimeSpan(30 * amount, 0, 0, 0);
			}
			else if (unit == 'y')
			{
				humanReadableDuration = amount + " year";
				timeSpanDuration = new TimeSpan(365 * amount, 0, 0, 0);
			}

			// Pluralize string if needed
			if (amount != 1)
			{
				humanReadableDuration += 's';
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
			userID = userID.Replace("@steam", "");
			steamName = null;

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

				XmlDocument xml = new XmlDocument();
				xml.LoadXml(xmlResponse);
				steamName = xml.DocumentElement?.SelectSingleNode("/profile/steamID")?.InnerText;

				response.Close();
				return !string.IsNullOrWhiteSpace(steamName);
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					Logger.Debug("Steam profile connection error: " + ((HttpWebResponse)e.Response).StatusCode);
				}
				else
				{
					Logger.Debug("Steam profile connection error: " + e.Status);
				}
			}
			finally
			{
				response?.Close();
			}
			return false;
		}

		private static bool SSLValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool isOk = true;
			// If there are errors in the certificate chain,
			// look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				for (int i = 0; i < chain.ChainStatus.Length; i++)
				{
					if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
					{
						continue;
					}
					chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
					chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
					chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
					bool chainIsValid = chain.Build((X509Certificate2)certificate);
					if (!chainIsValid)
					{
						isOk = false;
						break;
					}
				}
			}
			return isOk;
		}
	}
}
