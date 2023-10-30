using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	public class UnsyncCommand : ICommand
	{
		public string Command { get; } = "unsync";
		public string[] Aliases { get; } = new string[] { "us", "u" };
		public string Description { get; } = "Removes a user from having their discord role synced to the server.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.unsync"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}*/

			if (arguments.Count > 0 && ulong.TryParse(arguments.Array[2], out ulong discordID))
			{
				response = SCPDiscord.plugin.roleSync.RemovePlayerLocally(discordID);
				return true;
			}

			response = "Invalid arguments.";
			return false;
		}
	}
}