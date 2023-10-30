using System;
using CommandSystem;
using PluginAPI.Core;

namespace SCPDiscord.Commands
{
	[CommandHandler(typeof (GameConsoleCommandHandler))]
	public class ValidateCommand : ICommand
	{
		public string Command => "scpdiscord_validate";
		public string[] Aliases => new string[] { "scpd_validate" };
		public string Description => "Creates a config validation report.";
		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			/*if (sender is Player player)
			{
				if (!player.HasPermission("scpdiscord.validate"))
				{
					return new[] { "You don't have permission to use that command." };
				}
			}*/

			Config.ValidateConfig(SCPDiscord.plugin);
			Language.ValidateLanguageStrings();

			response = "Validation report posted in server console.";
			return true;
		}
	}
}