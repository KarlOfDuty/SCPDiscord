using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	public class GrantVanillaRankCommand : SCPDiscordCommand
	{
		public string Command { get; } = "grantvanillarank";
		public string[] Aliases { get; } = { "gvr" };
		public string Description { get; } = "Grants a player the vanilla rank provided.";
		public string[] ArgumentList { get; } = { "<playerid/steamid>", "<rank>" };

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*if (sender is Player admin)
			{
				if (!messages.HasPermission("scpdiscord.grantvanillarank"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}*/

			if (arguments.Count < 2)
			{
				response = "Invalid arguments.";
				return false;
			}

			string steamIDOrPlayerID = arguments.At(0).Replace("@steam", ""); // Remove steam suffix if there is one

			List<Player> matchingPlayers = new List<Player>();
			try
			{
				Logger.Debug("Looking for player with SteamID/PlayerID: " + steamIDOrPlayerID);
				foreach (Player pl in Player.GetPlayers<Player>())
				{
					Logger.Debug("Player " + pl.PlayerId + ": SteamID " + pl.UserId + " PlayerID " + pl.PlayerId);
					if (pl.GetParsedUserID() == steamIDOrPlayerID)
					{
						Logger.Debug("Matching SteamID found");
						matchingPlayers.Add(pl);
					}
					else if (pl.PlayerId.ToString() == steamIDOrPlayerID)
					{
						Logger.Debug("Matching playerID found");
						matchingPlayers.Add(pl);
					}
				}
			}
			catch (Exception) { /* ignored */ }

			if (!matchingPlayers.Any())
			{
				response = "Player \"" + arguments.At(0) + "\"not found.";
				return false;
			}

			try
			{
				foreach (Player matchingPlayer in matchingPlayers)
				{
					matchingPlayer.SetRank(null, null, arguments.At(1));
				}
			}
			catch (Exception)
			{
				response = "Vanilla rank \"" + arguments.At(1) + "\" not found. Are you sure you are using the RA config role name and not the role title/badge?";
				return false;
			}

			response = "Player rank updated.";
			return true;
		}
	}
}