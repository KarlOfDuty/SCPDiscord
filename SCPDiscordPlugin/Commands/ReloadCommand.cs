using System;
using CommandSystem;

namespace SCPDiscord.Commands
{
	public class ReloadCommand : ICommand
	{
		public string Command { get; } = "reload";
		public string[] Aliases { get; } = new string[] { "rl", "r" };
		public string Description { get; } = "Reloads all plugin configs and data files and then reconnects to the bot.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.reload"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}
			*/

			SCPDiscord.plugin.Info("Reloading plugin...");
			if (!SCPDiscord.plugin.LoadConfig())
			{
				response = "Reload failed.";
				return true;
			}

			Language.Reload();
			SCPDiscord.plugin.roleSync.Reload();
			if (NetworkSystem.IsConnected())
			{
				NetworkSystem.Disconnect();
			}

			response = "Reload complete.";
			return true;
		}
	}
}