using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	[CommandHandler(typeof (GameConsoleCommandHandler))]
	public class SetNickname : ICommand
	{
		public string Command => "scpdiscord_setnickname";
		public string[] Aliases => new string[] { "scpd_setnickname" };
		public string Description => "Sets a nickname for a player.";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*
			if (sender is Player admin)
			{
				if (!messages.HasPermission("scpdiscord.setnickname"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}
			*/

			if (arguments.Count <= 1)
			{
				response = "Invalid arguments.";
				return false;
			}

			string steamIDOrPlayerID = arguments.At(0).Replace("@steam", ""); // Remove steam suffix if there is one

			List<Player> matchingPlayers = new List<Player>();
			try
			{
				SCPDiscord.plugin.Debug("Looking for player with SteamID/PlayerID: " + steamIDOrPlayerID);
				foreach (Player pl in Player.GetPlayers<Player>())
				{
					SCPDiscord.plugin.Debug("Player " + pl.PlayerId + ": SteamID " + pl.UserId + " PlayerID " + pl.PlayerId);
					if (pl.GetParsedUserID() == steamIDOrPlayerID)
					{
						SCPDiscord.plugin.Debug("Matching SteamID found");
						matchingPlayers.Add(pl);
					}
					else if (pl.PlayerId.ToString() == steamIDOrPlayerID)
					{
						SCPDiscord.plugin.Debug("Matching playerID found");
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

			foreach (Player matchingPlayer in matchingPlayers)
			{
				matchingPlayer.DisplayNickname = string.Join(" ", arguments.Skip(1));
			}

			response = "Player nickname updated.";
			return true;
		}
	}
}