using System;
using CommandSystem;

namespace SCPDiscord.Commands
{
	public class DebugCommand : ICommand
	{
		public string Command { get; } = "debug";
		public string[] Aliases { get; } = new string[] { "d" };
		public string Description { get; } = "Toggles debug mode for SCPDiscord.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.debug"))
				{
					response = "You don't have permission to use that command.";
				}
			}
			*/
			Config.SetBool("settings.debug", !Config.GetBool("settings.debug"));
			response = "Debug messages: " + Config.GetBool("settings.debug");
			return true;
		}
	}
}