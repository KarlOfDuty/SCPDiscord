using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	public class ReconnectCommand : ICommand
	{
		public string Command { get; } = "reconnect";
		public string[] Aliases { get; } = new string[] { "rc" };
		public string Description { get; } = "Attempts to close the connection to the Discord bot and reconnect.";

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