using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	[CommandHandler(typeof (GameConsoleCommandHandler))]
	public class ReconnectCommand : ICommand
	{
		public string Command => "scpdiscord_reconnect";
		public string[] Aliases => new string[] { "scpd_rc", "scpd_reconnect", "scpdiscord_rc" };
		public string Description => "Attempts to close the connection to the Discord bot and reconnect.";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*
			if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.reconnect"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}
			*/

			if (NetworkSystem.IsConnected())
			{
				NetworkSystem.Disconnect();
				response = "Connection closed, reconnecting will begin shortly.";
				return true;
			}
			else
			{
				response = "Connection was already closed, reconnecting is in progress.";
				return false;
			}
		}
	}
}