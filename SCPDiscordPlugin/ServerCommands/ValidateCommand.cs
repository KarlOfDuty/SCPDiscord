using System;
using CommandSystem;

namespace SCPDiscord.Commands
{
  public class ValidateCommand : SCPDiscordCommand
  {
    public string Command { get; } = "validate";
    public string[] Aliases { get; } = { };
    public string Description { get; } = "Creates a config validation report.";
    public bool SanitizeResponse { get; } = true;
    public string[] ArgumentList { get; } = { };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      Logger.Debug(sender.LogName + " used the validate command.");

      Config.ValidateConfig(SCPDiscord.plugin);
      Language.ValidateLanguageStrings();

      response = "Validation report posted in server console.";
      return true;
    }
  }
}