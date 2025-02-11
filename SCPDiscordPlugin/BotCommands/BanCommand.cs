using System;
using System.Collections.Generic;
using LabApi.Events;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
  public static class BanCommand
  {
    public static void Execute(Interface.BanCommand command)
    {
      EmbedMessage embed = new EmbedMessage
      {
        Colour = EmbedMessage.Types.DiscordColour.Red,
        ChannelID = command.ChannelID,
        InteractionID = command.InteractionID
      };

      // Perform very basic SteamID validation.
      if (!Utilities.IsPossibleSteamID(command.SteamID, out ulong _))
      {
        Dictionary<string, string> variables = new Dictionary<string, string>
        {
          { "steamid",             command.SteamID },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.invalidsteamid", variables);
        return;
      }

      // Create duration timestamp.
      long durationSeconds = 0;
      long issuanceTime = DateTime.UtcNow.Ticks;
      DateTime endTime;
      try
      {
        endTime = Utilities.ParseCompoundDuration(command.Duration, ref durationSeconds);
      }
      catch (IndexOutOfRangeException)
      {
        endTime = DateTime.MinValue;
      }

      if (endTime == DateTime.MinValue)
      {
        Dictionary<string, string> variables = new Dictionary<string, string>
        {
          { "duration",            command.Duration },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.invalidduration", variables);
        return;
      }

      command.Reason = command.Reason.Replace(";", "");
      if (command.Reason == "")
      {
        command.Reason = "No reason provided.";
      }

      Dictionary<string, string> banVars = new Dictionary<string, string>
      {
        { "reason",              command.Reason },
        { "duration",            Utilities.SecondsToCompoundTime(durationSeconds) },
        { "discord-displayname", command.DiscordDisplayName },
        { "discord-username",    command.DiscordUsername },
        { "discord-userid",      command.DiscordUserID.ToString() },
      };

      if (!Utilities.TryGetPlayerName(command.SteamID, out string name))
      {
        if (!Utilities.TryGetSteamName(command.SteamID, out name))
        {
          name = "Offline player";
        }
      }

      name = name.Replace(";", "");

      // TODO: Feedback if the request is cancelled by another plugin

      // Send player banned event if player is online, and add ipban
      bool offlineBan = true;
      if (Utilities.TryGetPlayer(command.SteamID, out Player player))
      {
        offlineBan = false;
        banVars.AddPlayerVariables(player, "player");

        PlayerBanningEventArgs eventArgs = new PlayerBanningEventArgs(player.ReferenceHub, player.PlayerId.ToString(), Player.Host?.ReferenceHub, command.Reason, durationSeconds);
        PlayerEvents.OnBanning(eventArgs);
        if (!eventArgs.IsAllowed)
        {
          return;
        }

        BanHandler.IssueBan(new BanDetails
        {
          OriginalName = name,
          Id = player.ReferenceHub.connectionToClient.address,
          IssuanceTime = issuanceTime,
          Expires = endTime.Ticks,
          Reason = command.Reason,
          Issuer = command.DiscordUsername
        }, BanHandler.BanType.IP);
        ServerConsole.Disconnect(player.ReferenceHub.gameObject, "You have been banned. Reason: " + command.Reason);
      }

      BanHandler.IssueBan(new BanDetails
      {
        OriginalName = name,
        Id = (command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam"),
        IssuanceTime = issuanceTime,
        Expires = endTime.Ticks,
        Reason = command.Reason,
        Issuer = command.DiscordUsername
      }, BanHandler.BanType.UserId);

      BanHandler.ValidateBans();
      embed.Colour = EmbedMessage.Types.DiscordColour.Green;

      if (offlineBan)
      {
        banVars.Add("name", name);
        banVars.Add("userid", command.SteamID);
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playerbanned.offline", banVars);
      }
      else
      {
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playerbanned.online", banVars);
      }
    }
  }
}