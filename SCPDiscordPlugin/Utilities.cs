using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net.Http;
using LabApi.Features.Wrappers;
using Newtonsoft.Json.Linq;
using PlayerRoles;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SCPDiscord
{
  public static class Utilities
  {
    private static readonly HttpClient client = new HttpClient();

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
      if (seconds == 0) return "0 seconds";

      TimeSpan span = TimeSpan.FromSeconds(seconds);
      int[] parts = [span.Days / 365, span.Days % 365 / 31, span.Days % 365 % 31, span.Hours, span.Minutes, span.Seconds];
      string[] units = [" year", " month", " day", " hour", " minute", " second"];

      return string.Join(", ",
        from index in Enumerable.Range(0, units.Length)
        where parts[index] > 0
        select parts[index] + (parts[index] == 1 ? units[index] : units[index] + "s"));
    }

    public static string TicksToCompoundTime(long ticks)
    {
      return SecondsToCompoundTime(ticks / TimeSpan.TicksPerSecond);
    }

    public static bool TryGetPlayer(string userID, out Player pl)
    {
      foreach (Player player in Player.ReadyList)
      {
        string parsedUserID = player.GetParsedUserID();
        if (parsedUserID == null)
        {
          continue;
        }

        if (userID.Contains(player.GetParsedUserID()))
        {
          pl = player;
          return true;
        }
      }

      pl = null;
      return false;
    }

    public static bool TryGetPlayerName(string userID, out string name)
    {
      foreach (Player player in Player.ReadyList)
      {
        string parsedUserID = player.GetParsedUserID();
        if (parsedUserID == null)
        {
          continue;
        }

        if (userID.Contains(parsedUserID))
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
      messages.AddLast("");
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
      if (!IsPossibleSteamID(userID, out ulong userIDRaw))
      {
        return false;
      }

      string url = $"https://steamcommunity.com/profiles/{userIDRaw}?xml=1";
      try
      {
        string xmlResponse = client.GetStringAsync(url).Result;
        XmlDocument xml = new()
        {
          XmlResolver = null
        };
        xml.LoadXml(xmlResponse);
        steamName = xml.DocumentElement?.SelectSingleNode("/profile/steamID")?.InnerText;

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
      catch (Exception e)
      {
        Logger.Error($"Web request error ({url}): {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
      }

      return false;
    }

    public static string ReadManifestData(string embeddedFileName)
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      string resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

      using Stream stream = assembly.GetManifestResourceStream(resourceName);
      if (stream == null)
      {
        throw new InvalidOperationException("Could not load manifest resource stream.");
      }

      using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
      return reader.ReadToEnd();
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
