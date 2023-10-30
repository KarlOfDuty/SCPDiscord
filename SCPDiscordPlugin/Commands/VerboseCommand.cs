using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	[CommandHandler(typeof(GameConsoleCommandHandler))]
	public class VerboseCommand : ICommand
	{
		public string Command => "scpdiscord_verbose";
		public string[] Aliases => new string[] { "scpd_verbose" };
		public string Description => "Toggles verbose messages.";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Config.SetBool("settings.verbose", !Config.GetBool("settings.verbose"));
			response = "Verbose messages: " + Config.GetBool("settings.verbose");
			return true;
		}
	}
}