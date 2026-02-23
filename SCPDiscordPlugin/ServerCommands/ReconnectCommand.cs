using System;
using CommandSystem;

namespace SCPDiscord.Commands
{
  public class ReconnectCommand : SCPDiscordCommand
  {
    public string Command { get; } = "reconnect";
    public string[] Aliases { get; } = { "rc" };
    public string Description { get; } = "Attempts to close the connection to the Discord bot and reconnect.";
    public bool SanitizeResponse { get; } = true;
    public string[] ArgumentList { get; } = { };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
      Logger.Debug(sender.LogName + " used the reconnect command.");

      if (Network.IsConnected())
      {
        Network.Disconnect();
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