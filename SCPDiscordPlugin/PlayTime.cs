using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using Newtonsoft.Json;

namespace SCPDiscord
{
  public class TimeTrackingListener : CustomEventsHandler
  {
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
      if (!Config.GetBool("settings.playtime")
          || ev.Player?.UserId == null
          || ev.Player.PlayerId == Player.Host?.PlayerId)
      {
        return;
      }

      PlayTime.OnPlayerJoin(ev.Player.UserId, DateTime.Now);
    }

    public override void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
      if (!Config.GetBool("settings.playtime")
          || ev.Player?.UserId == null
          || ev.Player.PlayerId == Player.Host?.PlayerId)
      {
        return;
      }

      PlayTime.OnPlayerLeave(ev.Player.UserId);
    }

    public override void OnServerWaitingForPlayers()
    {
      PlayTime.WriteCacheToFile();
    }
  }

  public static class PlayTime
  {
    private static Dictionary<string, DateTime> joinTimes = new Dictionary<string, DateTime>();

    private static Utilities.FileWatcher fileWatcher;
    private static readonly object fileLock = new object();
    private static Dictionary<string, ulong> writeCache = new Dictionary<string, ulong>();
    private static Dictionary<string, ulong> playtimeData = new Dictionary<string, ulong>();

    public static void Reload()
    {
      lock (fileLock)
      {
        fileWatcher?.Dispose();

        if (!Directory.Exists(Config.GetPlaytimeDir()))
        {
          Directory.CreateDirectory(Config.GetPlaytimeDir());
        }

        if (!File.Exists(Config.GetPlaytimePath()))
        {
          Logger.Info("Playtime file " + Config.GetPlaytimePath() + "does not exist, creating...");
          File.WriteAllText(Config.GetPlaytimePath(), "{}");
        }

        playtimeData = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(File.ReadAllText(Config.GetPlaytimePath()));
        fileWatcher = new Utilities.FileWatcher(Config.GetPlaytimeDir(), "playtime.json", Reload);

        if (playtimeData == null)
        {
          Logger.Error("Failed loading \"" + Config.GetPlaytimePath() + "\".");
        }
        else
        {
          Logger.Debug("Reloaded \"" + Config.GetPlaytimePath() + "\".");
        }
      }
    }

    public static void OnPlayerJoin(string userID, DateTime joinTime)
    {
      if (!joinTimes.TryGetValue(userID, out _))
      {
        joinTimes.Add(userID, joinTime);
      }
    }

    public static void OnPlayerLeave(string userID)
    {
      if (!joinTimes.TryGetValue(userID, out DateTime joinTime))
      {
        return;
      }

      ulong seconds = (ulong)Math.Max(0, (DateTime.Now - joinTime).TotalSeconds);
      if (writeCache.ContainsKey(userID))
      {
        writeCache[userID] += seconds;
      }
      else
      {
        writeCache.Add(userID, seconds);
      }

      Logger.Debug("Player " + userID + " left after " + seconds + " seconds.");

      // Only remove the player from the list if they don't have several connections
      if (Player.List.Count(p => p.UserId == userID) < 2)
      {
        joinTimes.Remove(userID);
      }
    }

    public static void WriteCacheToFile()
    {
      if (!Config.GetBool("settings.playtime"))
      {
        writeCache = new Dictionary<string, ulong>();
        joinTimes = new Dictionary<string, DateTime>();
        return;
      }

      lock (fileLock)
      {
        try
        {
          fileWatcher?.Dispose();

          if (!Directory.Exists(Config.GetPlaytimeDir()))
          {
            Directory.CreateDirectory(Config.GetPlaytimeDir());
          }

          if (!File.Exists(Config.GetPlaytimePath()))
          {
            Logger.Info("Playtime file " + Config.GetPlaytimePath() + "does not exist, creating...");
            File.WriteAllText(Config.GetPlaytimePath(), "{}");
          }

          playtimeData = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(File.ReadAllText(Config.GetPlaytimePath()));
          foreach (KeyValuePair<string, ulong> pair in writeCache)
          {
            if (playtimeData.ContainsKey(pair.Key))
            {
              playtimeData[pair.Key] += pair.Value;
            }
            else
            {
              playtimeData.Add(pair.Key, pair.Value);
            }
          }

          writeCache = new Dictionary<string, ulong>();
          joinTimes = new Dictionary<string, DateTime>();
          File.WriteAllText(Config.GetPlaytimePath(), JsonConvert.SerializeObject(playtimeData, Formatting.Indented));
          fileWatcher = new Utilities.FileWatcher(Config.GetPlaytimeDir(), "playtime.json", Reload);
          Logger.Debug("Successfully wrote '" + Config.GetPlaytimePath() + "'.");
        }
        catch (Exception e)
        {
          switch (e)
          {
            case DirectoryNotFoundException _:
              Logger.Error("Time tracking file directory not found.");
              break;
            case UnauthorizedAccessException _:
              Logger.Error("Time tracking file '" + Config.GetPlaytimePath() + "' access denied.");
              break;
            case FileNotFoundException _:
              Logger.Error("Time tracking file '" + Config.GetPlaytimePath() + "' was not found.");
              break;
            case JsonReaderException _:
              Logger.Error("Time tracking file '" + Config.GetPlaytimePath() + "' formatting error.");
              break;
            default:
              Logger.Error("Error reading/writing time tracking file '" + Config.GetPlaytimePath() + "'.");
              break;
          }

          Logger.Error(e.ToString());
        }
      }
    }

    public static string GetHours(string userID)
    {
      TryGetHours(userID, out string hours);
      return hours;
    }

    public static bool TryGetHours(string userID, out string hours)
    {
      hours = "0.0";

      if (userID == null || playtimeData == null)
      {
        return false;
      }

      if (playtimeData.TryGetValue(userID, out ulong seconds))
      {
        hours = (seconds / 60.0 / 60.0).ToString("F1");
        return true;
      }

      return false;
    }
  }
}