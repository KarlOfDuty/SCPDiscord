using System.Collections.Generic;
using LabApi.Features.Wrappers;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
  public static class KickCommand
  {
    public static void Execute(Interface.KickCommand command)
    {
      EmbedMessage embed = new EmbedMessage
      {
        Colour = EmbedMessage.Types.DiscordColour.Red,
        ChannelID = command.ChannelID,
        InteractionID = command.InteractionID
      };

      //Perform very basic SteamID validation
      if (!Utilities.IsPossibleSteamID(command.SteamID, out ulong _))
      {
        Dictionary<string, string> vars = new Dictionary<string, string>
        {
          { "userid",              command.SteamID },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.invalidsteamid", vars);
        return;
      }

      //Get player name for feedback message
      if (!Utilities.TryGetPlayer(command.SteamID, out Player player))
      {
        Dictionary<string, string> vars = new Dictionary<string, string>
        {
          { "userid",              command.SteamID },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playernotfound", vars);
      }

      if (string.IsNullOrWhiteSpace(command.Reason))
      {
        command.Reason = "Kicked by server moderators";
      }

      //Kicks the player
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "reason",              command.Reason },
        { "discord-displayname", command.DiscordDisplayName },
        { "discord-username",    command.DiscordUsername },
        { "discord-userid",      command.DiscordUserID.ToString() },
      };
      variables.AddPlayerVariables(player, "player");
      embed.Colour = EmbedMessage.Types.DiscordColour.Green;

      player.Kick(command.Reason);
      SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playerkicked", variables);
    }
  }
}