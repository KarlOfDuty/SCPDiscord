using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	[CommandHandler(typeof(GameConsoleCommandHandler))]
	public class GrantReservedSlotCommand : ICommand
	{
		public string Command => "scpdiscord_grantreservedslot";
		public string[] Aliases => new[] { "scpd_grantreservedslot", "scpd_grs" };
		public string Description => "Adds a user to the reserved slots list and reloads it.";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*
			if (sender is Player admin)
			{
				if (!messages.HasPermission("scpdiscord.grantreservedslot"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}
			*/

			if (arguments.Count < 1)
			{
				response = "Invalid arguments.";
				return false;
			}

			string steamID = arguments.At(0).Trim();
			if (!steamID.EndsWith("@steam") && long.TryParse(steamID, out _))
			{
				steamID += "@steam";
			}

			if (!Regex.IsMatch(steamID, "[0-9]+@steam"))
			{
				response = "Invalid Steam ID provided!";
				return false;
			}

			string[] reservedSlotsFileRows = File.ReadAllLines(Config.GetReservedSlotPath());
			if (reservedSlotsFileRows.Any(row => row.Trim().StartsWith(steamID)))
			{
				response = "User already has a reserved slot!";
				return false;
			}

			if (arguments.Count > 1) // Add with comment
			{
				File.AppendAllLines(Config.GetReservedSlotPath(), new[] { "# SCPDiscord: " + string.Join(" ", arguments.Skip(1)), steamID });
			}
			else // Add without comment
			{
				File.AppendAllLines(Config.GetReservedSlotPath(), new[] { steamID });
			}
			ReservedSlot.Reload();
			response = "Reserved slot added.";
			return true;
		}
	}
}