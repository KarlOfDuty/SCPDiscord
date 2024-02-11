using System;
using System.Collections.Generic;
using System.Globalization;
using PlayerRoles;
using PluginAPI.Core;

namespace SCPDiscord
{
	public static class APIExtensions
	{
		public static string GetParsedUserID(this Player player)
		{
			if (!string.IsNullOrWhiteSpace(player.UserId))
			{
				int charLocation = player.UserId.LastIndexOf('@');

				if (charLocation > 0)
				{
					return player.UserId.Substring(0, charLocation);
				}
			}
			return null;
		}

		public static void SetRank(this Player player, string color = null, string text = null, string group = null)
		{
			if (player.ReferenceHub.serverRoles == null)
				return;
			if (group != null)
				player.ReferenceHub.serverRoles.SetGroup(ServerStatic.GetPermissionsHandler().GetGroup(group), false);
			if (color != null)
				player.ReferenceHub.serverRoles.SetColor(color);
			if (text != null)
				player.ReferenceHub.serverRoles.SetText(text);
		}

		public static bool TryGetRank(this Player player, out string rank)
		{
			// TODO: Update this when fixed by northwood
			if (!string.IsNullOrWhiteSpace(ServerStatic.GetPermissionsHandler()?.GetUserGroup(player.UserId)?.BadgeText))
			{
				rank = ServerStatic.GetPermissionsHandler().GetUserGroup(player.UserId).BadgeText;
				return true;
			}

			rank = "";
			return false;
		}

		public static string GetRank(this Player player)
		{
			return TryGetRank(player, out string rank) ? rank : "";
		}

		public static void AddPlayerVariables(this Dictionary<string, string> variables, Player player, string prefix, bool includeDisarmer = true)
		{
			if (variables == null) return;

			variables.AddIfNotExist(prefix + "-ipaddress",            player?.IpAddress);
			variables.AddIfNotExist(prefix + "-name",                 player?.Nickname);
			variables.AddIfNotExist(prefix + "-id",                   player?.PlayerId.ToString());
			variables.AddIfNotExist(prefix + "-userid",               player?.GetParsedUserID());
			variables.AddIfNotExist(prefix + "-role",                 player?.Role.ToString());
			variables.AddIfNotExist(prefix + "-team",                 player?.ReferenceHub.GetTeam().ToString());
			variables.AddIfNotExist(prefix + "-donottrack",           player?.DoNotTrack.ToString());
			variables.AddIfNotExist(prefix + "-health",               player?.Health.ToString(CultureInfo.InvariantCulture));
			variables.AddIfNotExist(prefix + "-isdisarmed",           player?.IsDisarmed.ToString());
			variables.AddIfNotExist(prefix + "-isalive",              player?.IsAlive.ToString());
			variables.AddIfNotExist(prefix + "-ismuted",              player?.IsMuted.ToString());
			variables.AddIfNotExist(prefix + "-hasreservedslot",      player?.HasReservedSlot.ToString());
			variables.AddIfNotExist(prefix + "-isglobalmod",          player?.IsGlobalModerator.ToString());
			variables.AddIfNotExist(prefix + "-isintercommuted",      player?.IsIntercomMuted.ToString());
			variables.AddIfNotExist(prefix + "-hasfullinventory",     player?.IsInventoryFull.ToString());
			variables.AddIfNotExist(prefix + "-hasnoitems",           player?.IsWithoutItems.ToString());
			variables.AddIfNotExist(prefix + "-isnortwoodstaff",      player?.IsNorthwoodStaff.ToString());
			variables.AddIfNotExist(prefix + "-overwatchon",          player?.IsOverwatchEnabled.ToString());
			variables.AddIfNotExist(prefix + "-noclipon",             player?.IsNoclipEnabled.ToString());
			variables.AddIfNotExist(prefix + "-godmodeon",            player?.IsGodModeEnabled.ToString());
			variables.AddIfNotExist(prefix + "-isoutofammo",          player?.IsOutOfAmmo.ToString());
			variables.AddIfNotExist(prefix + "-hasremoteadminaccess", player?.RemoteAdminAccess.ToString());
			variables.AddIfNotExist(prefix + "-bypassenabled",        player?.IsBypassEnabled.ToString());
			variables.AddIfNotExist(prefix + "-isscp",                player?.IsSCP.ToString());
			variables.AddIfNotExist(prefix + "-ismtf",                player?.IsNTF.ToString());
			variables.AddIfNotExist(prefix + "-ischaos",              player?.IsChaos.ToString());
			variables.AddIfNotExist(prefix + "-ishuman",              player?.IsHuman.ToString());
			variables.AddIfNotExist(prefix + "-rank",                 player?.GetRank());
			variables.AddIfNotExist(prefix + "-playtimehours",        TimeTracking.GetHours(player?.UserId));

			if (includeDisarmer)
			{
				variables.AddPlayerVariables(player?.DisarmedBy, prefix + "-disarmer", false);
			}
		}

		public static bool TryGetFirstKey<K, V>(this Dictionary<K, V> dict, V value, out K key)
		{
			key = default;

			foreach (KeyValuePair<K,V> pair in dict)
			{
				if (Comparer<V>.Default.Compare(pair.Value, value) == 0)
				{
					key = pair.Key;
					return true;
				}
			}

			return false;
		}

		public static void AddIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> variables, TKey key, TValue value)
		{
			try { variables.Add(key, value); }
			catch (ArgumentException) { }
		}
	}
}