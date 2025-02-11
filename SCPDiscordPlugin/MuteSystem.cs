using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using Newtonsoft.Json;
using VoiceChat;

namespace SCPDiscord
{
  public class MuteEventListener : CustomEventsHandler
  {
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
      if (ev.Player.PlayerId == Player.Host?.PlayerId) return;
      MuteSystem.CheckMuteStatus(ev.Player);
    }

    public override void OnPlayerMuted(PlayerMutedEventArgs ev)
    {
      if (ev.IsIntercom || ev?.Player.UserId == null)
      {
        return;
      }

      if (ev?.Player.UserId == MuteSystem.ignoreUserID)
      {
        Logger.Debug("Ignored mute event from self.");
        MuteSystem.ignoreUserID = "";
        return;
      }

      Logger.Debug("Caught mute event for " + ev.Player.UserId);

      string name = ev.Player.Nickname;
      MuteSystem.MutePlayer(ref name,
        ev.Player.UserId,
        ev.Issuer.Nickname + " (" + ev.Issuer.UserId + ")",
        "Muted using remote admin mute command.",
        DateTime.MaxValue);
    }

    public override void OnPlayerUnmuted(PlayerUnmutedEventArgs ev)
    {
      if (ev.IsIntercom || ev?.Player.UserId == null)
      {
        return;
      }

      if (ev?.Player.UserId == MuteSystem.ignoreUserID)
      {
        Logger.Debug("Ignored unmute event from self.");
        MuteSystem.ignoreUserID = "";
        return;
      }

      Logger.Debug("Caught unmute event for " + ev.Player.UserId);

      string name = ev.Player.Nickname;
      MuteSystem.UnmutePlayer(ref name,
        ev.Player.UserId,
        ev.Issuer.Nickname + " (" + ev.Issuer.UserId + ")");
    }
  }

  public static class MuteSystem
  {
    public static string ignoreUserID = "";
    private static Utilities.FileWatcher fileWatcher;
    private static object fileLock = new object();
    private static ConcurrentDictionary<ulong, MuteEntry> muteCache = new ConcurrentDictionary<ulong, MuteEntry>();

    private class MuteEntry
    {
      public string name;
      public string muter;
      public string unmuter;
      public string reason;
      public DateTime startTime;
      public DateTime endTime;
    }

    private static async Task SchedulePlayerCheck(DateTime targetTime, ulong steamID)
    {
      // Add one second to time to make sure the check triggers after the mute ends
      TimeSpan remaining = targetTime - DateTime.UtcNow + TimeSpan.FromSeconds(1);
      string userId = steamID + "@steam";
      if (remaining > TimeSpan.Zero)
      {
        await Task.Delay(remaining);
      }

      if (Utilities.TryGetPlayer(userId, out Player player))
      {
        CheckMuteStatus(player);
      }
    }

    public static bool IsMuted(string userID, out DateTime endTime, out string reason)
    {
      endTime = DateTime.UtcNow;
      reason = "";
      if (!Utilities.IsPossibleSteamID(userID, out ulong steamID))
      {
        return false;
      }

      if (muteCache.TryGetValue(steamID, out MuteEntry entry) && entry.endTime > DateTime.UtcNow)
      {
        endTime = entry.endTime;
        reason = entry.reason;
        return true;
      }

      if (VoiceChatMutes.QueryLocalMute(userID))
      {
        endTime = DateTime.MaxValue;
        return true;
      }

      return false;
    }

    public static bool MutePlayer(ref string playerName, string userID, string muter, string reason, DateTime endTime)
    {
      if (!Utilities.IsPossibleSteamID(userID, out ulong steamID))
      {
        return false;
      }

      if (muteCache.TryGetValue(steamID, out MuteEntry entry))
      {
        playerName = playerName.IsEmpty() ? entry.name : playerName;

        // Modify existing mute
        entry.name = playerName;
        entry.muter = muter;
        entry.unmuter = "";
        entry.reason = reason.IsEmpty() ? entry.reason : reason;
        entry.endTime = endTime;

        // If the player was not already muted set the start time, otherwise keep the old one
        if (entry.endTime < DateTime.UtcNow)
        {
          entry.startTime = DateTime.UtcNow;
        }
      }
      else
      {
        playerName = playerName.IsEmpty() ? "Offline Player" : playerName;

        // Create new mute entry
        MuteEntry newEntry = new MuteEntry
        {
          name = playerName,
          muter = muter,
          unmuter = "",
          reason = reason.IsEmpty() ? "" : reason,
          startTime = DateTime.UtcNow,
          endTime = endTime
        };

        muteCache[steamID] = newEntry;
      }

      VoiceChatMutes.IssueLocalMute(userID);
      Task.Run(() => SchedulePlayerCheck(endTime, steamID));
      Logger.Debug(playerName + " (" + userID + ") was muted ( " + endTime + " ).");
      return SaveMutes();
    }

    public static bool UnmutePlayer(ref string playerName, string userID, string unmuter)
    {
      if (!Utilities.IsPossibleSteamID(userID, out ulong steamID))
      {
        return false;
      }

      // Player has not been muted
      if (!muteCache.TryGetValue(steamID, out MuteEntry entry))
      {
        playerName = playerName.IsEmpty() ? entry.name : playerName;
        return true;
      }

      playerName = playerName.IsEmpty() ? entry.name : playerName;

      // Player is already unmuted
      if (entry.endTime < DateTime.UtcNow)
      {
        return true;
      }

      // Modify existing mute
      entry.name = playerName;
      entry.unmuter = unmuter;
      entry.endTime = DateTime.UtcNow;

      VoiceChatMutes.RevokeLocalMute(userID);
      Logger.Debug(playerName + " (" + userID + ") was unmuted.");
      return SaveMutes();
    }

    public static void CheckMuteStatus(Player player)
    {
      if (!Utilities.IsPossibleSteamID(player.UserId, out ulong steamID))
      {
        return;
      }

      if (!muteCache.TryGetValue(steamID, out MuteEntry entry))
      {
        return;
      }

      if (player.IsMuted && entry.endTime < DateTime.UtcNow)
      {
        Logger.Debug("Unmuting player " + player.Nickname + " (" + player.UserId + ")");
        player.Unmute(true);
      }
      else if (!player.IsMuted && entry.endTime > DateTime.UtcNow)
      {
        Logger.Debug("Muting player " + player.Nickname + " (" + player.UserId + ")");
        player.Mute(false);
      }
    }

    public static void Reload()
    {
      lock (fileLock)
      {
        try
        {
          fileWatcher?.Dispose();

          if (!Directory.Exists(Config.GetMutesDir()))
          {
            Directory.CreateDirectory(Config.GetMutesDir());
          }

          if (!File.Exists(Config.GetMutesPath()))
          {
            Logger.Info("Mute file \"" + Config.GetMutesPath() + "\" does not exist, creating...");
            File.WriteAllText(Config.GetMutesPath(), "{}");
          }

          fileWatcher = new Utilities.FileWatcher(Config.GetMutesDir(), "mutes.json", Reload);
          muteCache = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, MuteEntry>>(File.ReadAllText(Config.GetMutesPath()));
          Logger.Debug("Reloaded \"" + Config.GetMutesPath() + "\".");
        }
        catch (Exception e)
        {
          switch (e)
          {
            case DirectoryNotFoundException _:
              Logger.Error("Mute file directory not found.");
              break;
            case UnauthorizedAccessException _:
              Logger.Error("Mute file \"" + Config.GetMutesPath() + "\" access denied.");
              break;
            case FileNotFoundException _:
              Logger.Error("Mute file \"" + Config.GetMutesPath() + "\" was not found.");
              break;
            case JsonReaderException _:
              Logger.Error("Mute file \"" + Config.GetMutesPath() + "\" formatting error.");
              break;
            default:
              Logger.Error("Error reading mute file '" + Config.GetMutesPath() + "'.");
              break;
          }

          Logger.Error(e.ToString());
        }
      }
    }

    private static bool SaveMutes()
    {
      lock (fileLock)
      {
        try
        {
          File.WriteAllText(Config.GetMutesPath(), JsonConvert.SerializeObject(muteCache, Formatting.Indented));
          return true;
        }
        catch (Exception e)
        {
          Logger.Error("Failed writing to mute file \"" + Config.GetMutesPath() + "\".");
          Logger.Error(e.ToString());
          return false;
        }
      }
    }
  }
}
