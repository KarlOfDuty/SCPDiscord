using System;
using System.Collections.Generic;
using System.IO;
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
            TimeTracking.OnPlayerJoin(ev.Player.UserId, DateTime.Now);
        }

        [PluginEvent]
        public void OnPlayerLeave(PlayerLeftEvent ev)
        {
            if (ev.Player.PlayerId == Server.Instance.PlayerId) return;
			TimeTracking.OnPlayerLeave(ev.Player.UserId);
        }

        [PluginEvent]
        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
	        TimeTracking.WriteCache();
        }
    }

    public static class TimeTracking
    {
        private static Dictionary<string, DateTime> joinTimes = new Dictionary<string, DateTime>();
        private static Dictionary<string, ulong> writeCache = new Dictionary<string, ulong>();

        private static Dictionary<string, ulong> readCache = new Dictionary<string, ulong>();

        public static void OnPlayerJoin(string userID, DateTime joinTime)
        {
            joinTimes.Add(userID, joinTime);
        }

        public static void OnPlayerLeave(string userID)
        {
	        if (joinTimes.TryGetValue(userID, out DateTime joinTime))
	        {
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
        }

        public static void WriteCache()
        {
            try
            {
                if (!Directory.Exists(Config.GetTimeTrackingDir()))
                {
            	    Directory.CreateDirectory(Config.GetTimeTrackingDir());
                }

                if (!File.Exists(Config.GetTimeTrackingPath()))
                {
            	    Logger.Info("Time tracking file " + Config.GetTimeTrackingPath() + "does not exist, creating...");
            	    File.WriteAllText(Config.GetTimeTrackingPath(), "{}");
                }

                readCache = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(File.ReadAllText(Config.GetTimeTrackingPath()));
                foreach (KeyValuePair<string,ulong> pair in writeCache)
                {
	                if (readCache.ContainsKey(pair.Key))
	                {
		                readCache[pair.Key] += pair.Value;
	                }
	                else
	                {
		                readCache.Add(pair.Key, pair.Value);
	                }
                }

                writeCache = new Dictionary<string, ulong>();
                joinTimes = new Dictionary<string, DateTime>();
                File.WriteAllText(Config.GetTimeTrackingPath(), JsonConvert.SerializeObject(readCache, Formatting.Indented));
                Logger.Debug("Successfully wrote '" + Config.GetTimeTrackingPath() + "'.");
            }
            catch (Exception e)
            {
                switch (e)
                {
            	    case DirectoryNotFoundException _:
            		    Logger.Error("Time tracking file directory not found.");
            		    break;
            	    case UnauthorizedAccessException _:
            		    Logger.Error("Time tracking file '" + Config.GetTimeTrackingPath() + "' access denied.");
            		    break;
            	    case FileNotFoundException _:
            		    Logger.Error("Time tracking file '" + Config.GetTimeTrackingPath() + "' was not found.");
            		    break;
            	    case JsonReaderException _:
            		    Logger.Error("Time tracking file '" + Config.GetTimeTrackingPath() + "' formatting error.");
            		    break;
            	    default:
            		    Logger.Error("Error reading/writing time tracking file '" + Config.GetTimeTrackingPath() + "'.");
            		    break;
                }
                Logger.Error(e.ToString());
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

	        if (readCache.TryGetValue(userID, out ulong seconds))
	        {
		        hours = (seconds / 60.0 / 60.0).ToString("F1");
		        return true;
	        }

	        return false;
        }
    }
}