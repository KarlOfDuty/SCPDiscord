using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameCore;
using PluginAPI.Helpers;
using YamlDotNet.Serialization;
using Resources = SCPDiscord.Properties.Resources;

namespace SCPDiscord
{
	public static class Config
	{
		public class ConfigParseException : Exception
		{
			public ConfigParseException(Exception e) : base(e.Message, e) { }
		}

		public static bool ready;

		private static readonly Dictionary<string, string> configStrings = new Dictionary<string, string>
		{
			{ "bot.activity.active", ""          },
			{ "bot.activity.empty",  ""          },
			{ "bot.activity.full",   ""          },
			{ "bot.ip",              "127.0.0.1" },
			{ "bot.status.active",   ""          },
			{ "bot.status.empty",    ""          },
			{ "bot.status.full",     ""          },
			{ "settings.language",   "english"   },
			{ "settings.timestamp",  ""          }
		};

		private static readonly Dictionary<string, bool> configBools = new Dictionary<string, bool>
		{
			{ "channelsettings.invertfilters",            false },
			{ "settings.autoreload.reservedslots",        true  },
			{ "settings.autoreload.whitelist",            true  },
			{ "settings.configvalidation",                true  },
			{ "settings.debug",                           true  },
			{ "settings.emotes",                          true  },
			{ "settings.regeneratelanguagefiles",         false },
			{ "settings.rolesync",                        false },
			{ "settings.playtime",                        true  },
			{ "settings.useglobaldirectory.language",     true  },
			{ "settings.useglobaldirectory.mutes",        true  },
			{ "settings.useglobaldirectory.rolesync",     true  },
			{ "settings.useglobaldirectory.playtime",     true  },
			//{ "settings.autoreload.mutes",            true  }
		};

		private static readonly Dictionary<string, int> configInts = new Dictionary<string, int>
		{
			{ "bot.port", 8888 }
		};

		// The message arrays have to be entered separately as they are used in the language files as well
		private static readonly Dictionary<string, string[]> generalConfigArrays = new Dictionary<string, string[]>
		{
			{ "channelsettings.filterips",      new string[]{} },
			{ "channelsettings.filtersteamids", new string[]{} }
		};

		// The following four are a bit messed up but the language and config systems need slightly different versions of this list so it had to be this way
		private static readonly IReadOnlyList<string> configMessageArrays = new List<string>
		{
			"messages.connectedtobot",
			"messages.on079addexp",
			"messages.on079cancellockdown",
			"messages.on079levelup",
			"messages.on079lockdoor",
			"messages.on079lockdown",
			"messages.on079teslagate",
			"messages.on079unlockdoor",
			"messages.onban.player",
			"messages.onban.server",
			"messages.onbanissued.ip",
			"messages.onbanissued.userid",
			"messages.onbanrevoked.ip",
			"messages.onbanrevoked.userid",
			"messages.onbanupdated.ip",
			"messages.onbanupdated.userid",
			"messages.oncallcommand.console.player",
			"messages.oncallcommand.console.server",
			"messages.oncallcommand.game.player",
			"messages.oncallcommand.game.server",
			"messages.oncallcommand.remoteadmin.player",
			"messages.oncallcommand.remoteadmin.server",
			"messages.onconnect",
			"messages.ondecontaminate",
			"messages.ondetonate",
			"messages.ondooraccess.allowed",
			"messages.ondooraccess.denied",
			"messages.onelevatoruse",
			"messages.onexecutedcommand.console.player",
			"messages.onexecutedcommand.console.server",
			"messages.onexecutedcommand.game.player",
			"messages.onexecutedcommand.game.server",
			"messages.onexecutedcommand.remoteadmin.player",
			"messages.onexecutedcommand.remoteadmin.server",
			"messages.ongeneratoractivated",
			"messages.ongeneratorclose",
			"messages.ongeneratordeactivated",
			"messages.ongeneratorfinish",
			"messages.ongeneratoropen",
			"messages.ongeneratorunlock",
			"messages.ongrenadeexplosion",
			"messages.ongrenadehitplayer",
			"messages.onhandcuff.default",
			"messages.onhandcuff.nootherplayer",
			"messages.onhandcuffremoved.default",
			"messages.onhandcuffremoved.nootherplayer",
			"messages.onitemuse",
			"messages.onkick.player",
			"messages.onkick.server",
			"messages.onnicknameset",
			"messages.onplayercheaterreport",
			"messages.onplayerdie.default",
			"messages.onplayerdie.friendlyfire",
			"messages.onplayerdie.nokiller",
			"messages.onplayerdropammo",
			"messages.onplayerdropitem",
			"messages.onplayerescape",
			"messages.onplayerhurt.default",
			"messages.onplayerhurt.friendlyfire",
			"messages.onplayerhurt.noattacker",
			"messages.onplayerjoin",
			"messages.onplayerleave",
			"messages.onplayermuted.player.intercom",
			"messages.onplayermuted.player.standard",
			"messages.onplayermuted.server.intercom",
			"messages.onplayermuted.server.standard",
			"messages.onplayerpickupammo",
			"messages.onplayerpickuparmor",
			"messages.onplayerpickupitem",
			"messages.onplayerpickupscp330",
			"messages.onplayerradioswitch",
			"messages.onplayerreport",
			"messages.onplayertriggertesla.default",
			"messages.onplayertriggertesla.ignored",
			"messages.onplayerunmuted.player.intercom",
			"messages.onplayerunmuted.player.standard",
			"messages.onplayerunmuted.server.intercom",
			"messages.onplayerunmuted.server.standard",
			"messages.onpocketdimensionenter",
			"messages.onpocketdimensionexit",
			"messages.onrecallzombie",
			"messages.onreload",
			"messages.onroundend",
			"messages.onroundrestart",
			"messages.onroundstart",
			"messages.onscp914activate",
			"messages.onscp914changeknob",
			"messages.onsetrole.died",
			"messages.onsetrole.escaped",
			"messages.onsetrole.latejoin",
			"messages.onsetrole.left",
			"messages.onsetrole.other",
			"messages.onsetrole.remoteadmin",
			"messages.onsetrole.respawn",
			"messages.onsetrole.revived",
			"messages.onsetrole.roundstart",
			"messages.onsetservername",
			"messages.onspawn",
			"messages.onspawnragdoll",
			"messages.onstartcountdown.player.initiated",
			"messages.onstartcountdown.player.resumed",
			"messages.onstartcountdown.server.initiated",
			"messages.onstartcountdown.server.resumed",
			"messages.onstopcountdown.default",
			"messages.onstopcountdown.noplayer",
			"messages.onsummonvehicle.chaos",
			"messages.onsummonvehicle.mtf",
			"messages.onteamrespawn.ci",
			"messages.onteamrespawn.mtf",
			"messages.onthrowprojectile",
			"messages.onwaitingforplayers",
		};

		private static readonly IReadOnlyList<string> languageOnlyNodes = new List<string>
		{
			"messages.botactivity.active",
			"messages.botactivity.empty",
			"messages.botactivity.full",
			"messages.consolecommandfeedback",
			"messages.invalidduration",
			"messages.invalidsteamid",
			"messages.invalidsteamidorip",
			"messages.kickall",
			"messages.list.default.title",
			"messages.list.default.row.default",
			"messages.list.default.row.empty",
			"messages.list.ranked.title",
			"messages.list.ranked.row.default",
			"messages.list.ranked.row.empty",
			"messages.list.synced.title.online-only",
			"messages.list.synced.title.all",
			"messages.list.synced.row.online-only.default",
			"messages.list.synced.row.online-only.empty",
			"messages.list.synced.row.all.default",
			"messages.list.synced.row.all.empty",
			"messages.playerbanned",
			"messages.playerkicked",
			"messages.playermuted",
			"messages.playernotfound",
			"messages.playertempmuted",
			"messages.playerunbanned",
			"messages.playerunmuted",
		};

		internal static readonly IReadOnlyList<string> languageNodes = configMessageArrays.Concat(languageOnlyNodes).ToList();

		// Convert message nodes to a dictionary and combine it with the other config arrays, I am aware this is jank af
		private static readonly Dictionary<string, string[]> configArrays =
			// Convert the config message array to a dictionary of arrays
			configMessageArrays.Zip(new string[configMessageArrays.Count][], (name, emptyArray) => (name: name, emptyArray: emptyArray))
				.ToDictionary(ns => ns.name, ns => ns.emptyArray)
			// Add general config arrays
			.Concat(generalConfigArrays).ToDictionary(e => e.Key, e => e.Value);

		private static readonly Dictionary<string, Dictionary<string, ulong>> configDicts = new Dictionary<string, Dictionary<string, ulong>>
		{
			{ "channels", new Dictionary<string, ulong>() }
		};

		internal static Dictionary<ulong, string[]> roleDictionary = new Dictionary<ulong, string[]>();

		internal static void Reload(SCPDiscord plugin)
		{
			ready = false;

			if (!Directory.Exists(GetConfigDir()))
			{
				Directory.CreateDirectory(GetConfigDir());
			}

			if (!File.Exists(GetConfigPath()))
			{
				Logger.Info("Config file '" + GetConfigPath() + "' does not exist, creating...");
				File.WriteAllText(GetConfigPath(), Encoding.UTF8.GetString(Resources.config));
			}

			// Reads file contents into FileStream
			FileStream stream = File.OpenRead(GetConfigDir() + "config.yml");

			// Converts the FileStream into a YAML Dictionary object
			IDeserializer deserializer = new DeserializerBuilder().Build();
			object yamlObject = deserializer.Deserialize(new StreamReader(stream));

			// Converts the YAML Dictionary into JSON String
			ISerializer serializer = new SerializerBuilder()
				.JsonCompatible()
				.Build();
			string jsonString = serializer.Serialize(yamlObject);

			JObject json = JObject.Parse(jsonString);

			Logger.Debug("Reading config validation");

			// Reads the configvalidation node first as it is used for reading the others
			try
			{
				configBools["settings.configvalidation"] = json.SelectToken("settings.configvalidation").Value<bool>();
			}
			catch (ArgumentNullException)
			{
				Logger.Warn("Config bool 'settings.configvalidation' not found, using default value: true");
			}

			// Read config strings
			foreach (KeyValuePair<string, string> node in configStrings.ToList())
			{
				try
				{
					Logger.Debug("Reading config string '" + node.Key + "'");
					configStrings[node.Key] = json.SelectToken(node.Key).Value<string>();
				}
				catch (ArgumentNullException)
				{
					Logger.Warn("Config string '" + node.Key + "' not found, using default value: \"" + node.Value + "\"");
				}
				catch (Exception e)
				{
					Logger.Error("Reading config string '" + node.Key + "' failed: " + e.Message);
					throw new ConfigParseException(e);
				}
			}

			// Read config ints
			foreach (KeyValuePair<string, int> node in configInts.ToList())
			{
				try
				{
					Logger.Debug("Reading config int '" + node.Key + "'");
					configInts[node.Key] = json.SelectToken(node.Key).Value<int>();
				}
				catch (ArgumentNullException)
				{
					Logger.Warn("Config int '" + node.Key + "' not found, using default value: \"" + node.Value + "\"");
				}
				catch (Exception e)
				{
					Logger.Error("Reading config int '" + node.Key + "' failed: " + e.Message);
					throw new ConfigParseException(e);
				}
			}

			// Read config bools
			foreach (KeyValuePair<string, bool> node in configBools.ToList().Where(kvm => kvm.Key != "settings.configvalidation"))
			{
				try
				{
					Logger.Debug("Reading config bool '" + node.Key + "'");
					configBools[node.Key] = json.SelectToken(node.Key).Value<bool>();
				}
				catch (ArgumentNullException)
				{
					Logger.Warn("Config bool '" + node.Key + "' not found, using default value: " + node.Value);
				}
				catch (Exception e)
				{
					Logger.Error("Reading config bool '" + node.Key + "' failed: " + e.Message);
					throw new ConfigParseException(e);
				}
			}


			// Read config arrays
			foreach (KeyValuePair<string, string[]> node in configArrays.ToList())
			{
				try
				{
					Logger.Debug("Reading config array '" + node.Key + "'");
					configArrays[node.Key] = json.SelectToken(node.Key).Value<JArray>().Values<string>().ToArray();
				}
				catch (ArgumentNullException)
				{
					Logger.Warn("Config array '" + node.Key + "' not found, using default value: []");
				}
				catch (Exception e)
				{
					Logger.Error("Reading config arrays '" + node.Key + "' failed: " + e.Message);
					throw new ConfigParseException(e);
				}
			}

			// Read config dictionaries
			foreach (KeyValuePair<string, Dictionary<string, ulong>> node in configDicts.ToList())
			{
				try
				{
					Logger.Debug("Reading config dict '" + node.Key + "'");
					configDicts[node.Key] = json.SelectToken(node.Key).Value<JArray>().ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<ulong>());
				}
				catch (ArgumentNullException)
				{
					Logger.Warn("Config dictionary '" + node.Key + "' not found, using default value: []");
				}
				catch (Exception e)
				{
					Logger.Error("Reading config dict '" + node.Key + "' failed: " + e.Message);
					throw new ConfigParseException(e);
				}
			}

			// Read rolesync system
			if (GetBool("settings.rolesync"))
			{
				try
				{
					Logger.Debug("Reading rolesync");
					roleDictionary = json.SelectToken("rolesync").Value<JArray>().ToDictionary(k => ulong.Parse(((JObject)k).Properties().First().Name), v => v.Values().First().Value<JArray>().Values<string>().ToArray());
				}
				catch (Exception)
				{
					Logger.Error("The rolesync config list is invalid, rolesync disabled.");
					SetBool("settings.rolesync", false);
				}
			}

			Logger.Debug("Finished reading config file");

			if (GetBool("settings.configvalidation"))
			{
				ValidateConfig(plugin);
			}

			ready = true;
		}

		public static bool GetBool(string node)
		{
			return configBools[node];
		}

		public static string GetString(string node)
		{
			return configStrings[node];
		}

		public static int GetInt(string node)
		{
			return configInts[node];
		}

		public static string[] GetArray(string node)
		{
			return configArrays[node];
		}

		public static Dictionary<string, ulong> GetDict(string node)
		{
			return configDicts[node];
		}

		public static void SetBool(string key, bool value)
		{
			configBools[key] = value;
		}

		public static void SetString(string key, string value)
		{
			configStrings[key] = value;
		}

		public static void SetInt(string key, int value)
		{
			configInts[key] = value;
		}

		public static void SetArray(string key, string[] value)
		{
			configArrays[key] = value;
		}

		public static void SetDict(string key, Dictionary<string, ulong> value)
		{
			configDicts[key] = value;
		}

		public static string GetSCPSLConfigDir()
		{
			return Paths.SecretLab + "/";
		}

		public static string GetUserIDBansFile()
		{
			return BanHandler.GetPath(BanHandler.BanType.UserId);
		}

		public static string GetIPBansFile()
		{
			return BanHandler.GetPath(BanHandler.BanType.IP);
		}

		public static string GetConfigDir()
		{
			return Paths.LocalPlugins.Plugins + "/SCPDiscord/";
		}

		public static string GetConfigPath()
		{
			return GetConfigDir() + "config.yml";
		}

		public static string GetLanguageDir()
		{
			if (GetBool("settings.useglobaldirectory.language"))
			{
				return Paths.GlobalPlugins.Plugins + "/SCPDiscord/Languages/";
			}
			else
			{
				return Paths.LocalPlugins.Plugins + "/SCPDiscord/Languages/";
			}
		}

		public static string GetRolesyncDir()
		{
			if (GetBool("settings.useglobaldirectory.rolesync"))
			{
				return Paths.GlobalPlugins.Plugins + "/SCPDiscord/";
			}
			else
			{
				return Paths.LocalPlugins.Plugins + "/SCPDiscord/";
			}
		}

		public static string GetRolesyncPath()
		{
			return GetRolesyncDir() + "rolesync.json";
		}

		public static string GetMutesDir()
		{
			if (GetBool("settings.useglobaldirectory.mutes"))
			{
				return Paths.GlobalPlugins.Plugins + "/SCPDiscord/";
			}
			else
			{
				return Paths.LocalPlugins.Plugins + "/SCPDiscord/";
			}
		}

		public static string GetMutesPath()
		{
			return GetMutesDir() + "mutes.json";
		}

		public static string GetPlaytimeDir()
		{
			if (GetBool("settings.useglobaldirectory.playtime"))
			{
				return Paths.GlobalPlugins.Plugins + "/SCPDiscord/";
			}
			else
			{
				return Paths.LocalPlugins.Plugins + "/SCPDiscord/";
			}
		}

		public static string GetPlaytimePath()
		{
			return GetPlaytimeDir() + "playtime.json";
		}

		public static string GetReservedSlotDir()
		{
			// From ConfigSharing.Reload
			return ConfigSharing.Paths[3];
		}

		public static string GetReservedSlotPath()
		{
			// From ConfigSharing.Reload
			return ConfigSharing.Paths[3] + "UserIDReservedSlots.txt";
		}

		public static void ValidateConfig(SCPDiscord plugin)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("\n||||||||||||| SCPDiscord config validator ||||||||||||||\n");
			sb.Append("------------ Config strings ------------\n");
			foreach (KeyValuePair<string, string> node in configStrings)
			{
				sb.Append(node.Key + ": " + node.Value + "\n");
			}

			sb.Append("------------ Config ints ------------\n");
			foreach (KeyValuePair<string, int> node in configInts)
			{
				sb.Append(node.Key + ": " + node.Value + "\n");
			}

			sb.Append("------------ Config bools ------------\n");
			foreach (KeyValuePair<string, bool> node in configBools)
			{
				sb.Append(node.Key + ": " + node.Value + "\n");
			}

			sb.Append("------------ Config arrays ------------\n");
			foreach (KeyValuePair<string, string[]> node in configArrays)
			{
				sb.Append(node.Key + ": [ " + string.Join(", ", node.Value) + " ]\n");
				if (node.Key.StartsWith("messages."))
				{
					foreach (string s in node.Value)
					{
						if (!GetDict("channels").ContainsKey(s))
						{
							sb.Append("WARNING: Channel alias '" + s + "' does not exist!\n");
						}
					}
				}
			}

			sb.Append("------------ Config dictionaries ------------\n");
			foreach (KeyValuePair<string, Dictionary<string, ulong>> node in configDicts)
			{
				sb.Append(node.Key + ":\n");
				foreach (KeyValuePair<string, ulong> subNode in node.Value)
				{
					sb.Append("    " + subNode.Key + ": " + subNode.Value + "\n");
				}
			}

			sb.Append("------------ Rolesync system ------------\n");
			foreach (KeyValuePair<ulong, string[]> node in roleDictionary)
			{
				sb.Append(node.Key + ":\n");
				foreach (string command in node.Value)
				{
					sb.Append("    " + command + "\n");
				}
			}

			sb.Append("|||||||||||| End of config validation ||||||||||||");
			Logger.Info(sb.ToString());
		}

		public static List<ulong> GetChannelIDs(string path)
		{
			List<ulong> channelIDs = new List<ulong>();
			foreach (string alias in GetArray(path))
			{
				if (GetDict("channels").TryGetValue(alias, out ulong channelID))
				{
					channelIDs.Add(channelID);
				}
			}
			return channelIDs;
		}
	}
}