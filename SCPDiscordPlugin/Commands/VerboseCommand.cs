using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	public class VerboseCommand : ICommand
	{
		public string Command { get; } = "verbose";
		public string[] Aliases { get; } = new string[] { "" };
		public string Description { get; } = "Toggles verbose messages.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Config.SetBool("settings.verbose", !Config.GetBool("settings.verbose"));
			response = "Verbose messages: " + Config.GetBool("settings.verbose");
			return true;
		}
	}
}