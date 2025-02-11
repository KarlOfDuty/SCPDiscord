using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LabApi.Features.Wrappers;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SCPDiscord
{
  internal static class Language
  {
    internal static bool ready;

    private static JObject primary;
    private static JObject backup;
    private static JObject overrides;
    private static JObject emotes;
    private static JObject emoteOverrides;

    // All default languages included in the .dll
    private static readonly Dictionary<string, string> defaultLanguages = new Dictionary<string, string>
    {
      { "brazilian-portuguese", Utilities.ReadManifestData("Languages.brazilian-portuguese.yml") },
      { "emote-overrides",      Utilities.ReadManifestData("Languages.emote-overrides.yml")      },
      { "emotes",               Utilities.ReadManifestData("Languages.emotes.yml")               },
      { "english",              Utilities.ReadManifestData("Languages.english.yml")              },
      { "german",               Utilities.ReadManifestData("Languages.german.yml")               },
      { "italian",              Utilities.ReadManifestData("Languages.italian.yml")              },
      { "overrides",            Utilities.ReadManifestData("Languages.overrides.yml")            },
      { "russian",              Utilities.ReadManifestData("Languages.russian.yml")              },
      { "simplified-chinese",   Utilities.ReadManifestData("Languages.simplified-chinese.yml")   },
      { "ukrainian",            Utilities.ReadManifestData("Languages.ukrainian.yml")            },
    };

    internal static void Reload()
    {
      ready = false;

      if (!Directory.Exists(Config.GetLanguageDir()))
      {
        Directory.CreateDirectory(Config.GetLanguageDir());
      }

      // Save default language files
      SaveDefaultLanguages();

      // Read primary language file
      LoadLanguageFile(Config.GetString("settings.language"), "primary language", out primary);

      // Read backup language file if not the same as the primary
      if (Config.GetString("settings.language") != "english")
      {
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
      PrintLanguageCoverage();

      ready = true;
    }

    // TODO: Clean this function up
    internal static string GetProcessedMessage(string messagePath, Dictionary<string, string> variables)
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
        default:
          // Ignore
          break;
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
        if (!File.Exists(Config.GetLanguageDir() + language.Key + ".yml") || (Config.GetBool("settings.regeneratelanguagefiles")
                                                                              && language.Key != "overrides" && language.Key != "emote-overrides"))
        {
          Logger.Debug("Creating file \"" + Config.GetLanguageDir() + language.Key + ".yml\"...");
          try
          {
            File.WriteAllText((Config.GetLanguageDir() + language.Key + ".yml"), language.Value);
          }
          catch (DirectoryNotFoundException)
          {
            Logger.Warn("Could not create language file: Language directory does not exist, attempting to create it... ");
            Directory.CreateDirectory(Config.GetLanguageDir());
            Logger.Info("Creating language file " + Config.GetLanguageDir() + language.Key + ".yml...");
            File.WriteAllText((Config.GetLanguageDir() + language.Key + ".yml"), language.Value);
          }
        }
      }
    }

    private static void LoadLanguageFile(string language, string type, out JObject dataObject)
    {
      try
      {
        Logger.Debug("Loading " + type + " file...");
        dataObject = Utilities.LoadYamlFile(Config.GetLanguageDir() + language + ".yml");
        Logger.Info("Loaded " + type + " file \"" + Config.GetLanguageDir() + language + ".yml\".");
      }
      catch (Exception e)
      {
        switch (e)
        {
          case DirectoryNotFoundException _:
            Logger.Error("Language directory not found.");
            break;
          case UnauthorizedAccessException _:
            Logger.Error("Language file \"" + Config.GetLanguageDir() + language + ".yml\" access denied.");
            break;
          case FileNotFoundException _:
            Logger.Error("Language file \"" + Config.GetLanguageDir() + language + ".yml\" was not found.");
            break;
          case JsonReaderException _:
          case YamlException _:
            Logger.Error("Language file \"" + Config.GetLanguageDir() + language + ".yml\" formatting error.");
            break;
          default:
            Logger.Error("Error reading language file \"" + Config.GetLanguageDir() + language + ".yml\".");
            break;
        }

        Logger.Error(e.ToString());
        dataObject = new JObject();
      }
    }

    internal static void ValidateLanguageStrings()
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
        Logger.Info("No language errors.");
      }
    }

    internal static void PrintLanguageCoverage()
    {
      foreach (KeyValuePair<string, string> lang in defaultLanguages)
      {
        if (lang.Key.Contains("overrides") || lang.Key.Contains("emote"))
        {
          continue;
        }

        // Converts the FileStream into a YAML Dictionary object
        IDeserializer deserializer = new DeserializerBuilder().Build();
        object yamlObject = deserializer.Deserialize(new StringReader(lang.Value));

        // Converts the YAML Dictionary into JSON String
        ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();

        if (yamlObject == null)
        {
          Logger.Error("Could not deserialize YAML: '" + lang.Key + "'. Is it a valid YAML file?");
          continue;
        }

        string jsonString = serializer.Serialize(yamlObject);
        JObject language = JObject.Parse(jsonString);

        int workingNodes = 0;
        foreach (string node in Config.languageNodes)
        {
          try
          {
            if (language.SelectToken(node + ".message")?.Value<string>() != null)
            {
              ++workingNodes;
            }
          }
          catch (Exception) { /* ignore */ }
        }

        Logger.Info((lang.Key + ": ").PadRight(25) + (Math.Round((float)workingNodes / Config.languageNodes.Count * 100) + "%").PadLeft(5) + " (" + workingNodes + "/" + Config.languageNodes.Count + " nodes)");
      }
    }

    /// <summary>
    /// Gets a string from the primary or backup language file
    /// </summary>
    /// <param name="path">The path to the node</param>
    /// <returns></returns>
    private static string GetString(string path)
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
        if (primaryException is NullReferenceException
         || primaryException is ArgumentNullException
         || primaryException is InvalidCastException
         || primaryException is JsonException)
        {
          Logger.Warn("Error reading string \"" + path + "\" from primary language file, switching to backup...");
          try
          {
            return backup.SelectToken(path).Value<string>();
          }
          // The node also does not exist in the backup file
          catch (NullReferenceException)
          {
            Logger.Error("Error: Language string \"" + path + "\" does not exist. Message can not be sent.");
            return null;
          }
          catch (ArgumentNullException)
          {
            Logger.Error("Error: Language string \"" + path + "\" does not exist. Message can not be sent.");
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
    private static Dictionary<string, string> GetRegexDictionary(string path)
    {
      if (primary == null && backup == null)
      {
        Logger.Warn("Tried to read regex dictionary \"" + path + "\" before loading languages.");
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

      Logger.Warn("Error: Language regex dictionary \"" + path + "\" does not exist in language file.");
      return new Dictionary<string, string>();
    }

    private static List<string> GetCancelRegexList(string path)
    {
      if (primary == null && backup == null)
      {
        Logger.Warn("Tried to read cancel regex array \"" + path + "\" before loading languages.");
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

      Logger.Warn("Error: Language cancel regex array \"" + path + "\" does not exist in language file.");
      return new List<string>();
    }

    private static string GetEmote(string path)
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
        Logger.Warn("Emote string \"" + path + "\" does not exist.");
        return "";
      }
      catch (ArgumentNullException)
      {
        Logger.Warn("Emote string \"" + path + "\" does not exist.");
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

    internal static string RunFilters(ulong channelID, string ipAddress, string parsedUserID, string userIDReplacement, string message)
    {
      if (ShouldFilterIP(channelID) && !string.IsNullOrWhiteSpace(ipAddress))
      {
        message = message.Replace(ipAddress, new string('#', ipAddress.Length));
      }

      if (ShouldFilterSteamID(channelID) && !string.IsNullOrWhiteSpace(parsedUserID))
      {
        message = message.Replace(parsedUserID, userIDReplacement ?? "");
      }

      return message;
    }

    internal static string RunFilters(ulong channelID, Player player, string message)
    {
      if (ShouldFilterIP(channelID) && !string.IsNullOrWhiteSpace(player.IpAddress))
      {
        message = message.Replace(player.IpAddress, new string('#', player.IpAddress.Length));
      }

      if (ShouldFilterSteamID(channelID) && !string.IsNullOrWhiteSpace(player.GetParsedUserID()))
      {
        message = message.Replace(player.GetParsedUserID(), "Player " + player.PlayerId);
      }

      return message;
    }

    internal static string RunFilters(ulong channelID, string message)
    {
      bool filterIPs = ShouldFilterIP(channelID);
      bool filterSteamIDs = ShouldFilterSteamID(channelID);
      foreach (Player player in Player.List)
      {
        if (filterIPs && !string.IsNullOrWhiteSpace(player.IpAddress))
        {
          message = message.Replace(player.IpAddress, new string('#', player.IpAddress.Length));
        }

        if (filterSteamIDs && !string.IsNullOrWhiteSpace(player.GetParsedUserID()))
        {
          message = message.Replace(player.GetParsedUserID(), "Player " + player.PlayerId);
        }
      }

      return message;
    }

    private static bool ShouldFilterIP(ulong channelID)
    {
      bool filterIPs = Config.GetChannelIDs("channelsettings.filterips").Contains(channelID);
      if (Config.GetBool("channelsettings.invertipfilter"))
      {
        return !filterIPs;
      }

      return filterIPs;
    }

    private static bool ShouldFilterSteamID(ulong channelID)
    {
      bool filterIDs = Config.GetChannelIDs("channelsettings.filtersteamids").Contains(channelID);
      if (Config.GetBool("channelsettings.invertsteamidfilter"))
      {
        return !filterIDs;
      }

      return filterIDs;
    }

    private static string RunUsernameRegexReplacements(string input)
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