using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SCPDiscord.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PluginAPI.Core;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SCPDiscord
{
	internal static class Language
	{
		public static bool ready;

		private static JObject primary;
		private static JObject backup;
		private static JObject overrides;
		private static JObject emotes;
		private static JObject emoteOverrides;

		private static string languagesPath = Config.GetLanguageDir();

		// All default languages included in the .dll
		private static readonly Dictionary<string, string> defaultLanguages = new Dictionary<string, string>
		{
			{ "overrides",          Encoding.UTF8.GetString(Resources.overrides)          },
			{ "emotes",             Encoding.UTF8.GetString(Resources.emotes)             },
			{ "emote-overrides",    Encoding.UTF8.GetString(Resources.emote_overrides)    },
			{ "english",            Encoding.UTF8.GetString(Resources.english)            },
			{ "ukrainian",          Encoding.UTF8.GetString(Resources.ukrainian)          },
			{ "russian",            Encoding.UTF8.GetString(Resources.russian)            },
			{ "simplified-chinese", Encoding.UTF8.GetString(Resources.simplified_chinese) },
			{ "italian",            Encoding.UTF8.GetString(Resources.italian)            }
		};

		public static void Reload()
		{
			ready = false;
			languagesPath = Config.GetLanguageDir();

			if (!Directory.Exists(languagesPath))
			{
				Directory.CreateDirectory(languagesPath);
			}

			// Save default language files
			SaveDefaultLanguages();

			// Read primary language file
			Logger.Info("Loading primary language file...");
			LoadLanguageFile(Config.GetString("settings.language"), "primary language", out primary);

			// Read backup language file if not the same as the primary
			if (Config.GetString("settings.language") != "english")
			{
				Logger.Info("Loading backup language file...");
				LoadLanguageFile("english", "backup language", out backup);
			}

			if (primary == null && backup == null)
			{
				Logger.Error("NO LANGUAGE FILE LOADED! DEACTIVATING SCPDISCORD.");
				throw new Exception();
			}

			LoadLanguageFile("overrides", "language overrides", out overrides);
			LoadLanguageFile("emotes", "emote", out emotes);
			LoadLanguageFile("emote-overrides", "emote overrides", out emoteOverrides);
			ValidateLanguageStrings();

			ready = true;
		}

		// TODO: Clean this function up
		public static string GetProcessedMessage(string messagePath, Dictionary<string, string> variables)
		{
			// Get unparsed message from config
			string message;
			try
			{
				message = GetString(messagePath + ".message");
			}
			catch (Exception e)
			{
				Logger.Error("Error reading base message" + e);
				return null;
			}

			if (Config.GetBool("settings.emotes"))
			{
				message = GetEmote(messagePath) + message;
			}

			switch (message)
			{
				// An error message is already sent in the language function if this is null, so this just returns
				case null:
					return null;
				// Abort on empty message
				case "":
				case " ":
				case ".":
					Logger.Warn("Tried to send empty message '" + messagePath + "' to discord. Check your language files.");
					return null;
			}

			// Re-add newlines
			message = message.Replace("\\n", "\n");

			// Add variables //////////////////////////////
			if (variables != null)
			{
				// Variable insertion
				foreach (KeyValuePair<string, string> variable in variables)
				{
					// Wait until after the regex replacements to add the player names
					if (variable.Key == "name"
					 || variable.Key == "attacker-name"
					 || variable.Key == "player-name"
					 || variable.Key == "disarmer-name"
					 || variable.Key == "target-name"
					 || variable.Key == "issuer-name")
					{
						continue;
					}
					message = message.Replace("<var:" + variable.Key + ">", variable.Value);
				}
			}
			///////////////////////////////////////////////

			// Global regex replacements //////////////////
			Dictionary<string, string> globalRegex;
			try
			{
				globalRegex = GetRegexDictionary("global_regex");
			}
			catch (Exception e)
			{
				Logger.Error("Error reading global regex" + e);
				return null;
			}
			// Run the global regex replacements
			foreach (KeyValuePair<string, string> entry in globalRegex)
			{
				message = Regex.Replace(message, entry.Key, entry.Value);
			}
			///////////////////////////////////////////////

			// Local regex replacements ///////////////////
			Dictionary<string, string> localRegex;
			try
			{
				localRegex = GetRegexDictionary(messagePath + ".regex");
			}
			catch (Exception e)
			{
				Logger.Error("Error reading local regex" + e);
				return null;
			}
			// Run the local regex replacements
			foreach (KeyValuePair<string, string> entry in localRegex)
			{
				message = Regex.Replace(message, entry.Key, entry.Value);
			}
			///////////////////////////////////////////////

			// Regex cancel checks ////////////////////////
			List<string> cancelRegex;
			try
			{
				cancelRegex = GetCancelRegexList(messagePath + ".cancel_regex");
			}
			catch (Exception e)
			{
				Logger.Error("Error reading local regex" + e);
				return null;
			}
			// Run the regex cancel checks
			foreach (string entry in cancelRegex)
			{
				if (Regex.IsMatch(message, entry))
				{
					return null;
				}
			}
			///////////////////////////////////////////////

			if (variables != null)
			{
				// Add names/command feedback to the message //
				foreach (KeyValuePair<string, string> variable in variables)
				{
					message = message.Replace("<var:" + variable.Key + ">", RunUsernameRegexReplacements(variable.Value ?? "null"));
				}
				///////////////////////////////////////////////

				// Final regex replacements ///////////////////
				Dictionary<string, string> finalRegex;
				try
				{
					finalRegex = Language.GetRegexDictionary("final_regex");
				}
				catch (Exception e)
				{
					Logger.Error("Error reading final regex" + e);
					return null;
				}
				// Run the final regex replacements
				foreach (KeyValuePair<string, string> entry in finalRegex)
				{
					message = Regex.Replace(message, entry.Key, entry.Value);
				}
				///////////////////////////////////////////////
			}
			return message;
		}

		/// <summary>
		/// Saves all default language files included in the .dll
		/// </summary>
		private static void SaveDefaultLanguages()
		{
			foreach (KeyValuePair<string, string> language in defaultLanguages)
			{
				if (!File.Exists(languagesPath + language.Key + ".yml") || (Config.GetBool("settings.regeneratelanguagefiles")
				                                                            && language.Key != "overrides" && language.Key != "emote-overrides"))
				{
					Logger.Debug("Creating file " + languagesPath + language.Key + ".yml...");
					try
					{
						File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
					}
					catch (DirectoryNotFoundException)
					{
						Logger.Warn("Could not create language file: Language directory does not exist, attempting to create it... ");
						Directory.CreateDirectory(languagesPath);
						Logger.Info("Creating language file " + languagesPath + language.Key + ".yml...");
						File.WriteAllText((languagesPath + language.Key + ".yml"), language.Value);
					}
				}
			}
		}

		/// <summary>
		/// This function makes me want to die too, don't worry.
		/// Parses a yaml file into a yaml object, parses the yaml object into a json string, parses the json string into a json object
		/// </summary>
		private static void LoadLanguageFile(string language, string type, out JObject dataObject)
		{
			try
			{
				// Reads file contents into FileStream
                FileStream stream = File.OpenRead(languagesPath + language + ".yml");

                // Converts the FileStream into a YAML Dictionary object
                IDeserializer deserializer = new DeserializerBuilder().Build();
                object yamlObject = deserializer.Deserialize(new StreamReader(stream));

                // Converts the YAML Dictionary into JSON String
                ISerializer serializer = new SerializerBuilder()
                	.JsonCompatible()
                	.Build();
                string jsonString = serializer.Serialize(yamlObject);
                dataObject = JObject.Parse(jsonString);

                Logger.Info("Successfully loaded " + type + " file '" + language + ".yml'.");
			}
			catch (Exception e)
			{
				switch (e)
				{
					case DirectoryNotFoundException _:
						Logger.Warn("Language directory not found.");
						break;
					case UnauthorizedAccessException _:
						Logger.Warn("Language file '" + languagesPath + language + ".yml' access denied.");
						break;
					case FileNotFoundException _:
						Logger.Warn("Language file '" + languagesPath + language + ".yml' was not found.");
						break;
					case JsonReaderException _:
					case YamlException _:
						Logger.Warn("Language file '" + languagesPath + language + ".yml' formatting error.");
						break;
					default:
						Logger.Warn("Error reading language file '" + languagesPath + language + ".yml'.");
						break;
				}
				Logger.Error(e.ToString());
				dataObject = new JObject();
			}
		}

		public static void ValidateLanguageStrings()
		{
			bool valid = true;
			foreach (string node in Config.languageNodes)
			{
				try
				{
					primary.SelectToken(node + ".message").Value<string>();
				}
				catch (Exception)
				{
					Logger.Warn("Your SCPDiscord language file \"" + Config.GetString("settings.language") + ".yml\" does not contain the node \"" + node + ".message\".");
					valid = false;
				}
			}

			if (Config.GetString("settings.language") != "english")
			{
				foreach (string node in Config.languageNodes)
				{
					try
					{
						backup.SelectToken(node + ".message").Value<string>();
					}
					catch (Exception)
					{
						Logger.Warn("Your SCPDiscord backup language file \"english.yml\" does not contain the node \"" + node + ".message\".");
						valid = false;
					}
				}
			}

			foreach (string node in Config.languageNodes)
			{
				try
				{
					emotes.SelectToken(node).Value<string>();
				}
				catch (Exception)
				{
					Logger.Warn("The emote file \"emotes.yml\" does not contain the node \"" + node + "\".");
					valid = false;
				}
			}

			if (valid)
			{
				Logger.Info("No language errors.\n");
			}
		}

		/// <summary>
		/// Gets a string from the primary or backup language file
		/// </summary>
		/// <param name="path">The path to the node</param>
		/// <returns></returns>
		public static string GetString(string path)
		{
			if (primary == null && backup == null)
			{
				Logger.Warn("Tried to read language string before loading languages.");
				return null;
			}

			try
			{
				try
				{
					return overrides.SelectToken(path).Value<string>();
				}
				catch (Exception) { /* ignore */ }
				return primary.SelectToken(path).Value<string>();
			}
			catch (Exception primaryException)
			{
				// This exception means the node does not exist in the language file, the plugin attempts to find it in the backup file
				if (primaryException is NullReferenceException || primaryException is ArgumentNullException || primaryException is InvalidCastException || primaryException is JsonException)
				{
					Logger.Warn("Error reading string '" + path + "' from primary language file, switching to backup...");
					try
					{
						return backup.SelectToken(path).Value<string>();
					}
					// The node also does not exist in the backup file
					catch (NullReferenceException)
					{
						Logger.Error("Error: Language string '" + path + "' does not exist. Message can not be sent.");
						return null;
					}
					catch (ArgumentNullException)
					{
						Logger.Error("Error: Language string '" + path + "' does not exist. Message can not be sent.");
						return null;
					}
					catch (InvalidCastException e)
					{
						Logger.Error(e.ToString());
						throw;
					}
					catch (JsonException e)
					{
						Logger.Error(e.ToString());
						throw;
					}
				}
				else
				{
					Logger.Error(primaryException.ToString());
					throw;
				}
			}
		}

		/// <summary>
		/// Gets a regex dictionary from the primary or backup language file.
		/// </summary>
		/// <param name="path">The path to the node.</param>
		/// <returns></returns>
		public static Dictionary<string, string> GetRegexDictionary(string path)
		{
			if (primary == null && backup == null)
			{
				Logger.Warn("Tried to read regex dictionary '" + path + "' before loading languages.");
				return new Dictionary<string, string>();
			}

			try
			{
				try
				{
					JArray overrideArray = overrides?.SelectToken(path).Value<JArray>();
					return overrideArray?.ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
				}
				catch (Exception) { /* ignore */ }
				JArray primaryArray = primary?.SelectToken(path).Value<JArray>();
				return primaryArray?.ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
			}
			catch (NullReferenceException)
			{
				try
				{
					JArray backupArray = backup?.SelectToken(path).Value<JArray>();
					return backupArray?.ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<string>());
				}
				catch (NullReferenceException)
				{
					// Doesn't exist
				}
				catch (ArgumentNullException)
				{
					// Regex array is empty
					return new Dictionary<string, string>();
				}
				catch (InvalidCastException e)
				{
					Logger.Error(e.ToString());
					throw;
				}
				catch (JsonException e)
				{
					Logger.Error(e.ToString());
					throw;
				}
			}
			catch (ArgumentNullException)
			{
				// Regex array is empty
				return new Dictionary<string, string>();
			}
			catch (InvalidCastException e)
			{
				Logger.Error(e.ToString());
				throw;
			}
			catch (JsonException e)
			{
				Logger.Error(e.ToString());
				throw;
			}

			Logger.Warn("Error: Language regex dictionary '" + path + "' does not exist in language file.");
			return new Dictionary<string, string>();
		}

		public static List<string> GetCancelRegexList(string path)
		{
			if (primary == null && backup == null)
			{
				Logger.Warn("Tried to read cancel regex array '" + path + "' before loading languages.");
				return new List<string>();
			}

			try
			{
				try
				{
					return overrides.SelectToken(path).Value<JArray>().Values<string>().ToList();
				}
				catch (Exception) { /* ignore */ }
				return primary.SelectToken(path).Value<JArray>().Values<string>().ToList();
			}
			catch (NullReferenceException)
			{
				try
				{
					return backup.SelectToken(path).Value<JArray>().Values<string>().ToList();
				}
				catch (NullReferenceException)
				{
					// Doesn't exist
				}
				catch (ArgumentNullException)
				{
					// Regex array is empty
					return new List<string>();
				}
				catch (InvalidCastException e)
				{
					Logger.Error(e.ToString());
					throw;
				}
				catch (JsonException e)
				{
					Logger.Error(e.ToString());
					throw;
				}
			}
			catch (ArgumentNullException)
			{
				// Regex array is empty
				return new List<string>();
			}
			catch (InvalidCastException e)
			{
				Logger.Error(e.ToString());
				throw;
			}
			catch (JsonException e)
			{
				Logger.Error(e.ToString());
				throw;
			}

			Logger.Warn("Error: Language cancel regex array '" + path + "' does not exist in language file.");
			return new List<string>();
		}

		public static string GetEmote(string path)
		{
			if (emotes == null)
			{
				Logger.Warn("Tried to read emote string before loading languages.");
				return null;
			}

			try
			{
				try
				{
					return emoteOverrides.SelectToken(path).Value<string>();
				}
				catch (Exception) { /* ignore */ }
				return emotes.SelectToken(path).Value<string>();
			}
			catch (NullReferenceException)
			{
				Logger.Warn("Emote string '" + path + "' does not exist.");
				return "";
			}
			catch (ArgumentNullException)
			{
				Logger.Warn("Emote string '" + path + "' does not exist.");
				return "";
			}
			catch (InvalidCastException e)
			{
				Logger.Error(e.ToString());
				throw;
			}
			catch (JsonException e)
			{
				Logger.Error(e.ToString());
				throw;
			}
		}

		public static void RunFiltersManually(ulong channelID, string ipAddress, string parsedUserID, string userIDReplacement, ref string message)
		{
			if (Config.GetChannelIDs("channelsettings.filterips").Contains(channelID) && !string.IsNullOrWhiteSpace(ipAddress))
			{
				message = message.Replace(ipAddress, new string('#', ipAddress.Length));
			}

			if (Config.GetChannelIDs("channelsettings.filtersteamids").Contains(channelID) && !string.IsNullOrWhiteSpace(parsedUserID))
			{
				message = message.Replace(parsedUserID, userIDReplacement);
			}
		}

		public static void RunFilters(ulong channelID, Player player, ref string message)
		{
			if (Config.GetChannelIDs("channelsettings.filterips").Contains(channelID))
			{
				message = message.Replace(player.IpAddress, new string('#', player.IpAddress.Length));
			}

			if (Config.GetChannelIDs("channelsettings.filtersteamids").Contains(channelID))
			{
				message = message.Replace(player.GetParsedUserID(), "Player " + player.PlayerId);
			}
		}

		public static void RunFilters(ulong channelID, ref string message)
		{
			foreach (Player player in Player.GetPlayers())
			{
				if (Config.GetChannelIDs("channelsettings.filterips").Contains(channelID))
				{
					message = message.Replace(player.IpAddress, new string('#', player.IpAddress.Length));
				}

				if (Config.GetChannelIDs("channelsettings.filtersteamids").Contains(channelID))
				{
					message = message.Replace(player.GetParsedUserID(), "Player " + player.PlayerId);
				}
			}
		}

		public static string RunUsernameRegexReplacements(string input)
		{
			Dictionary<string, string> userRegex;
			try
			{
				userRegex = GetRegexDictionary("user_regex");
			}
			catch (Exception e)
			{
				Logger.Error("Error reading user regex" + e);
				return "null";
			}

			foreach (KeyValuePair<string, string> entry in userRegex)
			{
				input = Regex.Replace(input, entry.Key, entry.Value);
			}
			return input;
		}
	}
}
