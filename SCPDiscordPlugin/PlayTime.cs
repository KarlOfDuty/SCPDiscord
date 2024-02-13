using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace SCPDiscord
{
    public class TimeTrackingListener
    {
        [PluginEvent]
        public void OnPlayerJoin(PlayerJoinedEvent ev)
        {
            if (ev.Player.PlayerId == Server.Instance.PlayerId) return;
            PlayTime.OnPlayerJoin(ev.Player.UserId, DateTime.Now);
        }

        [PluginEvent]
        public void OnPlayerLeave(PlayerLeftEvent ev)
        {
            if (ev.Player.PlayerId == Server.Instance.PlayerId) return;
			PlayTime.OnPlayerLeave(ev.Player.UserId);
        }

        [PluginEvent]
        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
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
		        Logger.Debug("Successfully loaded '" + Config.GetPlaytimePath() + "'.");
	        }
        }

        public static void OnPlayerJoin(string userID, DateTime joinTime)
        {
            joinTimes.Add(userID, joinTime);
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
	        joinTimes.Remove(userID);
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
	                foreach (KeyValuePair<string,ulong> pair in writeCache)
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

	        if (userID == null)
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