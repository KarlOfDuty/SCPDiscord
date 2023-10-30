using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	[CommandHandler(typeof (GameConsoleCommandHandler))]
	public class UnsyncCommand : ICommand
	{
		public string Command => "scpdiscord_unsync";
		public string[] Aliases => new string[] { "scpd_unsync" };
		public string Description => "Removes a user from having their discord role synced to the server.";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.unsync"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}*/

			if (arguments.Count > 0 && ulong.TryParse(arguments.At(0), out ulong discordID))
			{
				response = SCPDiscord.plugin.roleSync.RemovePlayerLocally(discordID);
				return true;
			}

			response = "Invalid arguments.";
			return false;
		}
	}
}