using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using Utils.NonAllocLINQ;
using VoiceChat;

namespace SCPDiscord
{
	public class MuteEventListener
	{
		[PluginEvent]
		public void OnPlayerJoinMuteCheck(PlayerJoinedEvent ev)
		{
			if (ev.Player.PlayerId == Server.Instance.PlayerId) return;

			MuteSystem.CheckMuteStatus(ev.Player);
		}

		[PluginEvent]
		public void OnPlayerMuted(PlayerMutedEvent ev)
		{
			if (ev.IsIntercom || ev?.Player.UserId == null)
			{
				return;
			}

			if (ev?.Player.UserId == MuteSystem.ignoreUserID)
			{
				MuteSystem.ignoreUserID = "";
				return;
			}

			MuteSystem.MutePlayer(ev.Player.Nickname,
				                  ev.Player.UserId,
				                  ev.Issuer.DisplayNickname + " (" + ev.Issuer.UserId + ")",
				                  "Muted using remote admin mute command.",
				                  DateTime.MaxValue);
		}

		[PluginEvent]
		public void OnPlayerUnmuted(PlayerUnmutedEvent ev)
		{
			if (ev.IsIntercom || ev?.Player.UserId == null)
			{
				return;
			}

			if (ev?.Player.UserId == MuteSystem.ignoreUserID)
			{
				MuteSystem.ignoreUserID = "";
				return;
			}

			MuteSystem.UnmutePlayer(ev.Player.Nickname,
				                    ev.Player.UserId,
				                    ev.Issuer.DisplayNickname + " (" + ev.Issuer.UserId + ")");
		}
	}

	public class MuteFileReloader
	{
		public MuteFileReloader()
		{
			while (true)
			{
				MuteSystem.ReloadMutes();
				Thread.Sleep(60000);
			}
		}
	}

    public static class MuteSystem
    {
        public static string ignoreUserID = "";
		private static Mutex muteFileMutex = new Mutex();
		private static List<MuteEntry> muteCache = new List<MuteEntry>(); // TODO: Switch to thread safe container

		private class MuteEntry
        {
	        public string name;
	        public string userID;
	        public string muter;
	        public string unmuter;
	        public string reason;
	        public DateTime startTime;
	        public DateTime endTime;
        }

        public static bool MutePlayer(string playerName, string userID, string muter, string reason, DateTime endTime)
        {
	        ReloadMutes();

	        if (muteCache.TryGetFirst(x => x.userID == userID, out MuteEntry entry))
	        {
		        // Modify existing mute
		        entry.name = playerName.IsEmpty() ? entry.name : playerName;
		        entry.muter = muter;
		        entry.unmuter = "";
		        entry.reason = reason.IsEmpty() ? entry.reason : reason;
		        entry.endTime = endTime;

		        // If the player was not already muted set the start time, otherwise keep the old one
		        if (entry.endTime < DateTime.Now)
		        {
			        entry.startTime = DateTime.Now;
		        }
	        }
	        else
	        {
		        // Create new mute entry
		        MuteEntry newEntry = new MuteEntry
		        {
			        name = playerName.IsEmpty() ? "Offline Player" : playerName,
			        userID = userID,
			        muter = muter,
			        unmuter = "",
			        reason = reason.IsEmpty() ? "" : reason,
			        startTime = DateTime.Now,
			        endTime = endTime
		        };

		        muteCache.Add(newEntry);
	        }

			VoiceChatMutes.IssueLocalMute(userID);
			Logger.Debug(playerName + " (" + userID + ") was muted ( " + endTime.ToLocalTime() + " ).");
	        return SaveMutes();
        }

        public static bool UnmutePlayer(string playerName, string userID, string unmuter)
        {
	        ReloadMutes();

	        // Player has not been muted
	        if (!muteCache.TryGetFirst(x => x.userID == userID, out MuteEntry entry))
	        {
		        return true;
	        }

	        // Player is already unmuted
	        if (entry.endTime < DateTime.Now)
	        {
		        return true;
	        }

	        // Modify existing mute
	        entry.name = playerName.IsEmpty() ? entry.name : playerName;
	        entry.unmuter = unmuter;
	        entry.endTime = DateTime.Now;

			VoiceChatMutes.RevokeLocalMute(userID);
			Logger.Debug(playerName + " (" + userID + ") was unmuted.");
	        return SaveMutes();
        }

        public static void CheckMuteStatus(Player player)
        {
	        List<MuteEntry> muteCacheCopy = muteCache;

	        if (!muteCacheCopy.TryGetFirst(x => x.userID == player.UserId, out MuteEntry entry))
		        return;

	        if (player.IsMuted && entry.endTime < DateTime.Now)
	        {
		        player.Unmute(true);
	        }
	        else if (!player.IsMuted && entry.endTime > DateTime.Now)
	        {
		        player.Mute(false);
	        }
        }

        public static void ReloadMutes()
        {
	        if (!muteFileMutex.WaitOne(500))
	        {
		        Logger.Warn("Unable to open mute file to reload.");
		        return;
	        }

	        if (!TryLoadMutes(out List<MuteEntry> mutes))
	        {
		        Logger.Warn("Unable to open mute file to reload.");
		        muteFileMutex.ReleaseMutex();
		        return;
	        }

	        muteCache = mutes;
	        muteFileMutex.ReleaseMutex();
        }

        private static bool SaveMutes()
        {
	        if (!muteFileMutex.WaitOne(500))
	        {
		        Logger.Error("Unable to lock mute system, could not write the mute file!");
		        return false;
	        }

	        try
	        {
		        File.WriteAllText(Config.GetMutesPath(), JsonConvert.SerializeObject(muteCache));
		        muteFileMutex.ReleaseMutex();
		        return true;
	        }
	        catch (Exception e)
	        {
		        Logger.Error("Failed writing to mute file '" + Config.GetMutesPath() + "'.");
		        Logger.Error(e.ToString());
		        muteFileMutex.ReleaseMutex();
		        return false;
	        }
        }

	    private static bool TryLoadMutes(out List<MuteEntry> muteEntries)
	    {
		    try
		    {
			    if (!Directory.Exists(Config.GetMutesDir()))
			    {
				    Directory.CreateDirectory(Config.GetMutesDir());
			    }

			    if (!File.Exists(Config.GetMutesPath()))
			    {
				    Logger.Info("Registry file " + Config.GetMutesPath() + "does not exist, creating...");
				    File.WriteAllText(Config.GetMutesPath(), "[]");
			    }

			    muteEntries = JsonConvert.DeserializeObject<MuteEntry[]>(File.ReadAllText(Config.GetMutesPath())).ToList();
			    return true;
		    }
		    catch (Exception e)
		    {
			    switch (e)
			    {
				    case DirectoryNotFoundException _:
					    Logger.Error("Mute file directory not found.");
					    break;
				    case UnauthorizedAccessException _:
					    Logger.Error("Mute file '" + Config.GetMutesPath() + "' access denied.");
					    break;
				    case FileNotFoundException _:
					    Logger.Error("Mute file '" + Config.GetMutesPath() + "' was not found.");
					    break;
				    case JsonReaderException _:
					    Logger.Error("Mute file '" + Config.GetMutesPath() + "' formatting error.");
					    break;
				    default:
					    Logger.Error("Error reading mute file '" + Config.GetMutesPath() + "'.");
					    break;
			    }
			    Logger.Error(e.ToString());
			    muteEntries = null;
			    return false;
		    }
	    }
    }
}