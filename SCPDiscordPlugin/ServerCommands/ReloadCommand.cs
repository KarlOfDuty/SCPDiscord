using System;
using CommandSystem;

namespace SCPDiscord.Commands
{
  public class ReloadCommand : SCPDiscordCommand
  {
    public string Command { get; } = "reload";
    public string[] Aliases { get; } = { };
    public string Description { get; } = "Reloads all plugin configs and data files and then reconnects to the bot.";
    public bool SanitizeResponse { get; } = true;
    public string[] ArgumentList { get; } = { };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      Logger.Debug(sender.LogName + " used the reload command.");

      Logger.Info("Reloading plugin...");
      if (!SCPDiscord.plugin.LoadConfig())
      {
        response = "Reload failed.";
        return true;
      }

      Language.Reload();
      RoleSync.Reload();
      if (Network.socketWrapper.IsConnected())
      {
        Network.Disconnect();
      }

      response = "Reload complete.";
      return true;
    }
  }
}