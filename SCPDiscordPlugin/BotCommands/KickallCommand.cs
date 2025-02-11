using System.Collections.Generic;
using LabApi.Features.Wrappers;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
  public static class KickallCommand
  {
    public static void Execute(Interface.KickallCommand command)
    {
      if (command.Reason == "")
      {
        command.Reason = "All players kicked by Admin";
      }

      foreach (Player player in Player.List)
      {
        player.Ban(command.Reason, 0);
      }

      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "reason",              command.Reason },
        { "discord-displayname", command.DiscordDisplayName },
        { "discord-username",    command.DiscordUsername },
        { "discord-userid",      command.DiscordUserID.ToString() },
      };

      EmbedMessage embed = new EmbedMessage
      {
        Colour = EmbedMessage.Types.DiscordColour.Green,
        ChannelID = command.ChannelID,
        InteractionID = command.InteractionID
      };
      SCPDiscord.SendEmbedWithMessageByID(embed, "messages.kickall", variables);
    }
  }
}