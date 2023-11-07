using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	public class VerboseCommand : SCPDiscordCommand
	{
		public string Command { get; } = "verbose";
		public string[] Aliases { get; } = { };
		public string Description { get; } = "Toggles verbose messages.";
		public string[] ArgumentList { get; } = { };

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Config.SetBool("settings.verbose", !Config.GetBool("settings.verbose"));
			response = "Verbose messages: " + Config.GetBool("settings.verbose");
			return true;
		}
	}
}