using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameCore;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Paths;

namespace SCPDiscord
{
  public static class Config
  {
    public class ConfigParseException : Exception
    {
      public ConfigParseException(Exception e) : base(e.Message, e) {}
    }

    public static bool ready { get; private set; }

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
      { "settings.logfile",    ""          },
      { "settings.timestamp",  ""          }
    };

    private static readonly Dictionary<string, bool> configBools = new Dictionary<string, bool>
    {
      { "channelsettings.invertipfilter",       false },
      { "channelsettings.invertsteamidfilter",  false },
      { "settings.autoreload.reservedslots",    true  },
      { "settings.autoreload.whitelist",        true  },
      { "settings.configvalidation",            true  },
      { "settings.debug",                       true  },
      { "settings.emotes",                      true  },
      { "settings.regeneratelanguagefiles",     false },
      { "settings.rolesync",                    false },
      { "settings.playtime",                    true  },
      { "settings.useglobaldirectory.language", true  },
      { "settings.useglobaldirectory.mutes",    true  },
      { "settings.useglobaldirectory.rolesync", true  },
      { "settings.useglobaldirectory.playtime", true  },
      //{ "settings.autoreload.mutes",            true  }
    };

    private static readonly Dictionary<string, int> configInts = new Dictionary<string, int>
    {
      { "bot.port", 8888 }
    };

    // ///////////////////////////////////////////////////////////
    // The message arrays have to be entered separately as they are used in the language files as well
    // ///////////////////////////////////////////////////////////

    // These ones are only used in the config, not language files
    private static readonly Dictionary<string, string[]> generalConfigArrays = new Dictionary<string, string[]>
    {
      { "channelsettings.filterips",      new string[]{} },
      { "channelsettings.filtersteamids", new string[]{} }
    };

    // This is shared between the config and language files, however the language system needs a list of strings like this one here,
    // but the config system needs a dictionary of arrays where the entries in this list are the keys so it gets converted further down.
    private static readonly IReadOnlyList<string> configMessageArrays = new List<string>
    {
      "messages.connectedtobot",
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
      "messages.onconnect",
      "messages.ondecontaminate",
      "messages.ondetonate",
      "messages.onelevatoruse",
      "messages.onexecutedcommand.console.player",
      "messages.onexecutedcommand.console.server",
      "messages.onexecutedcommand.client.player",
      "messages.onexecutedcommand.client.server",
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
      "messages.oninteract330",
      "messages.onitemuse",
      "messages.onkick.player",
      "messages.onkick.server",
      "messages.onmapgenerated",
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
      "messages.onplayerinteractlocker",
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
      "messages.onplayerreceiveeffect",
      "messages.onplayerreport",
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
      "messages.onsetservername",
      "messages.onspawn",
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
      "messages.onserversentadminchat",
    };

    // Same as above but these are only used in language files.
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
      "messages.playerbanned.online",
      "messages.playerbanned.offline",
      "messages.playerkicked",
      "messages.playermuted",
      "messages.playernotfound",
      "messages.playertempmuted",
      "messages.playerunbanned",
      "messages.playerunmuted",
    };

    // This is the final list of all language file message nodes, combining the above lists.
    internal static readonly IReadOnlyList<string> languageNodes = configMessageArrays.Concat(languageOnlyNodes).ToList();

    // This is the janky part, this is a dictionary of all arrays used in the config file which has to convert the
    // configMessageArrays list to a dictionary of arrays and then add the other config arrays.
    private static readonly Dictionary<string, string[]> configArrays =
      // Convert the config message array to a dictionary of arrays
      configMessageArrays.Zip(new string[configMessageArrays.Count][], (name, emptyArray) => (name, emptyArray))
        .ToDictionary(ns => ns.name, ns => ns.emptyArray)
        // Add general config arrays
        .Concat(generalConfigArrays).ToDictionary(e => e.Key, e => e.Value);

    private static readonly Dictionary<string, Dictionary<string, ulong>> configDicts = new()
    {
      { "channels", new Dictionary<string, ulong>() }
    };

    internal static void Reload(SCPDiscord plugin)
    {
      ready = false;

      if (!Directory.Exists(GetConfigDir()))
      {
        Directory.CreateDirectory(GetConfigDir());
      }

      if (!File.Exists(GetConfigPath()))
      {
        Logger.Info("Config file \"" + GetConfigPath() + "\" does not exist, creating...");
        File.WriteAllText(GetConfigPath(), Utilities.ReadManifestData("config.yml"));
      }

      JObject json = Utilities.LoadYamlFile(GetConfigDir() + "config.yml");

      // Reads the debug node first as it is used for reading the others
      try
      {
        configBools["settings.debug"] = json.SelectToken("settings.debug").Value<bool>();
      }
      catch (ArgumentNullException)
      {
        Logger.Warn("Config bool 'settings.debug' not found, using default value: true");
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
      foreach (KeyValuePair<string, bool> node in configBools.ToList())
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
          Logger.Warn("Config array '" + node.Key + "' not found.");
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
          configDicts[node.Key] = json.SelectToken(node.Key).Value<JArray>().ToDictionary(
                                    k => ((JObject)k).Properties().First().Name,
                                    v => v.Values().First().Value<ulong>());
        }
        catch (ArgumentNullException)
        {
          Logger.Warn("Config dictionary '" + node.Key + "' not found.");
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
          JToken rolesyncToken = json.SelectToken("rolesync");

          if (rolesyncToken == null)
          {
            Logger.Error("Rolesync is enabled but no rolesync configuration could be found in the config.");
            SetBool("settings.rolesync", false);
          }
          else if (rolesyncToken.Type == JTokenType.Array) // Backwards compatibility for old rolesync config syntax
          {
            Logger.Warn("Rolesync config is in an old format, consider updating it to use newer features.");
            RoleSync.compatibilityMode = true;
            RoleSync.configCompat = rolesyncToken.Value<JArray>().ToDictionary(
                               k => ulong.Parse(((JObject)k).Properties().First().Name),
                               v => v.Values().First().Value<JArray>().Values<string>().ToArray());
          }
          else
          {
            RoleSync.compatibilityMode = false;
            RoleSync.config = rolesyncToken.ToObject<Dictionary<string, RoleSync.RoleCommands>>();
          }
        }
        catch (Exception e)
        {
          Logger.Error("The rolesync config list is invalid, rolesync disabled.");
          Logger.Debug(e.ToString());
          SetBool("settings.rolesync", false);
        }
      }

      Logger.Debug("Finished reading config file");

      if (GetBool("settings.configvalidation"))
      {
        ValidateConfig(plugin);
      }

      Logger.SetupLogfile(GetString("settings.logfile"));
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

    public static bool TryGetArray(string node, out string[] stringArray)
    {
      stringArray = configArrays[node];
      return stringArray != null;
    }

    public static bool TryGetDict(string node, out Dictionary<string, ulong> dict)
    {
      dict = configDicts[node];
      return dict != null;
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

    // TODO: Update paths with local/global paths

    public static string GetSCPSLConfigDir()
    {
      return PathManager.SecretLab + "/";
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
      return $"{SCPDiscord.plugin.GetConfigDirectory(false)}/";
    }

    public static string GetConfigPath()
    {
      return GetConfigDir() + "config.yml";
    }

    public static string GetLanguageDir()
    {
      if (GetBool("settings.useglobaldirectory.language"))
      {
        return $"{SCPDiscord.plugin.GetConfigDirectory(true)}/Languages/";
      }
      else
      {
        return $"{SCPDiscord.plugin.GetConfigDirectory(false)}/Languages/";
      }
    }

    public static string GetRolesyncDir()
    {
      if (GetBool("settings.useglobaldirectory.rolesync"))
      {
        return $"{SCPDiscord.plugin.GetConfigDirectory(true)}/";
      }
      else
      {
        return $"{SCPDiscord.plugin.GetConfigDirectory(false)}/";
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
        return $"{SCPDiscord.plugin.GetConfigDirectory(true)}/";
      }
      else
      {
        return $"{SCPDiscord.plugin.GetConfigDirectory(false)}/";
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
        return $"{SCPDiscord.plugin.GetConfigDirectory(true)}/";
      }
      else
      {
        return $"{SCPDiscord.plugin.GetConfigDirectory(false)}/";
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
      sb.Append("||||||||||||| SCPDISCORD CONFIG VALIDATOR ||||||||||||||\n");
      sb.Append("\n------------ Config strings ------------\n");
      foreach (KeyValuePair<string, string> node in configStrings)
      {
        sb.Append(node.Key + ": " + node.Value + "\n");
      }

      sb.Append("\n------------ Config ints ------------\n");
      foreach (KeyValuePair<string, int> node in configInts)
      {
        sb.Append(node.Key + ": " + node.Value + "\n");
      }

      sb.Append("\n------------ Config bools ------------\n");
      foreach (KeyValuePair<string, bool> node in configBools)
      {
        sb.Append(node.Key + ": " + node.Value + "\n");
      }

      sb.Append("\n------------ Config dictionaries ------------\n");
      foreach (KeyValuePair<string, Dictionary<string, ulong>> node in configDicts)
      {
        sb.Append(node.Key + ":\n");
        foreach (KeyValuePair<string, ulong> subNode in node.Value)
        {
          sb.Append("    " + subNode.Key + ": " + subNode.Value + "\n");
        }
      }

      if (TryGetDict("channels", out Dictionary<string, ulong> channelDict))
      {
        sb.Append("\n------------ Config arrays ------------\n");
        foreach (KeyValuePair<string, string[]> node in configArrays)
        {
          if (node.Value == null)
          {
            sb.Append(node.Key + ": NOT FOUND!\n");
            continue;
          }

          sb.Append(node.Key + ": [ " + string.Join(", ", node.Value ?? new[] { "ERROR - NOT FOUND" }) + " ]\n");
          if (node.Key.StartsWith("messages."))
          {
            foreach (string s in node.Value ?? Array.Empty<string>())
            {
              if (!channelDict.ContainsKey(s))
              {
                sb.Append("WARNING: Channel alias '" + s + "' does not exist!\n");
              }
            }
          }
        }
      }

      sb.Append("\n------------ Rolesync system ------------");
      if (RoleSync.compatibilityMode)
      {
        sb.Append("\n");
        foreach (KeyValuePair<ulong, string[]> node in RoleSync.configCompat)
        {
          sb.Append(node.Key + ":\n");
          foreach (string command in node.Value)
          {
            sb.Append("  - \"" + command + "\"\n");
          }
        }
      }
      else
      {
        ValidateRoleCommands(RoleSync.config, sb, 0);
      }

      sb.Append("|||||||||||| END OF CONFIG VALIDATION ||||||||||||");
      Logger.Info(sb.ToString());
      Logger.Info("You can turn the config validator off when you get the config set up correctly.");
    }

    private static void ValidateRoleCommands(Dictionary<string, RoleSync.RoleCommands> commands, StringBuilder sb, int indent)
    {
      if (commands == null || commands.Count == 0)
      {
        sb.Append(" {}\n");
        return;
      }

      string indentation = new(' ', indent);
      string subKeyIndentation = new(' ', indent + 2);

      sb.Append("\n");
      foreach (KeyValuePair<string, RoleSync.RoleCommands> command in commands)
      {
        sb.Append($"{indentation}{command.Key}:\n");
        sb.Append($"{subKeyIndentation}ids: [ {string.Join(", ", command.Value.roleIDs.Select(id => id.ToString()))} ]\n");
        sb.Append($"{subKeyIndentation}commands:\n");
        foreach (string cmd in command.Value.commands)
        {
          sb.Append($"{subKeyIndentation}  - \"{cmd}\"\n");
        }
        sb.Append($"{subKeyIndentation}sub-roles:");
        ValidateRoleCommands(command.Value.subRoles, sb, indent + 4);
      }
    }

    public static List<ulong> GetChannelIDs(string path)
    {
      if (!TryGetArray(path, out string[] aliasArray))
      {
        Logger.Warn("Tried to get \"" + path + "\" from config but it could not be found or was invalid.");
        return [];
      }

      if (!TryGetDict("channels", out Dictionary<string, ulong> dict))
      {
        Logger.Error("Tried to get channel aliases from config but they could not be found or were invalid.");
        return [];
      }

      List<ulong> channelIDs = [];
      foreach (string alias in aliasArray)
      {
        if (dict.TryGetValue(alias, out ulong channelID))
        {
          channelIDs.Add(channelID);
        }
      }

      return channelIDs;
    }
  }
}