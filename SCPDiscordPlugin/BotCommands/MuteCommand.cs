using System;
using System.Collections.Generic;
using LabApi.Events;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
  // This command is also used for unmuting by setting the duration to 0
  public static class MuteCommand
  {
    public static void Execute(Interface.MuteCommand command)
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
          { "steamid", command.SteamID }
        };
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.invalidsteamid", variables);
        return;
      }

      // Create duration timestamp.
      long durationSeconds = 0;
      DateTime endTime;

      if (command.Duration.ToLower().Trim().Contains("perm"))
      {
        endTime = DateTime.MaxValue;
      }
      else if (command.Duration.Trim() == "0")
      {
        endTime = DateTime.MinValue;
      }
      else
      {
        try
        {
          endTime = Utilities.ParseCompoundDuration(command.Duration.Trim(), ref durationSeconds);
        }
        catch (IndexOutOfRangeException)
        {
          endTime = DateTime.MinValue;
        }

        if (endTime == DateTime.MinValue)
        {
          Dictionary<string, string> variables = new Dictionary<string, string>
          {
            { "duration", command.Duration }
          };
          SCPDiscord.SendEmbedWithMessageByID(embed, "messages.invalidduration", variables);
          return;
        }
      }

      if (endTime > DateTime.UtcNow)
      {
        MutePlayer(command, endTime, Utilities.SecondsToCompoundTime(durationSeconds));
      }
      else
      {
        UnmutePlayer(command);
      }
    }

    private static void MutePlayer(Interface.MuteCommand command, DateTime endTime, string humanReadableDuration)
    {
      string userID = command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam";
      string playerName = "";

      if (Utilities.TryGetPlayer(userID, out Player player))
      {
        MuteSystem.ignoreUserID = userID;

        PlayerMutingEventArgs mutedEventArgs = new PlayerMutingEventArgs(player.ReferenceHub, Player.Host?.ReferenceHub, false);
        PlayerEvents.OnMuting(mutedEventArgs);
        if (mutedEventArgs.IsAllowed)
        {
          EmbedMessage embed = new EmbedMessage
          {
            Colour = EmbedMessage.Types.DiscordColour.Red,
            ChannelID = command.ChannelID,
            InteractionID = command.InteractionID
          };

          Dictionary<string, string> banVars = new Dictionary<string, string>
          {
            { "name",                playerName },
            { "userid",              command.SteamID },
            { "reason",              command.Reason },
            { "duration",            humanReadableDuration },
            { "discord-displayname", command.DiscordDisplayName },
            { "discord-username",    command.DiscordUsername },
            { "discord-userid",      command.DiscordUserID.ToString() },
          };

          SCPDiscord.SendEmbedWithMessageByID(embed, "messages.mutefailed", banVars);
          return;
        }

        playerName = player.Nickname;
      }
      else
      {
        // If the player is not online, check the steam api instead
        Utilities.TryGetSteamName(userID, out playerName);
      }

      if (MuteSystem.MutePlayer(ref playerName, userID, command.DiscordUsername, command.Reason, endTime))
      {
        EmbedMessage embed = new EmbedMessage
        {
          Colour = EmbedMessage.Types.DiscordColour.Green,
          ChannelID = command.ChannelID,
          InteractionID = command.InteractionID
        };

        Dictionary<string, string> banVars = new Dictionary<string, string>
        {
          { "name",                playerName },
          { "userid",              command.SteamID },
          { "reason",              command.Reason },
          { "duration",            humanReadableDuration },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };

        if (endTime == DateTime.MaxValue)
        {
          SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playermuted", banVars);
        }
        else
        {
          SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playertempmuted", banVars);
        }
      }
      else
      {
        EmbedMessage embed = new EmbedMessage
        {
          Colour = EmbedMessage.Types.DiscordColour.Red,
          ChannelID = command.ChannelID,
          InteractionID = command.InteractionID
        };

        Dictionary<string, string> banVars = new Dictionary<string, string>
        {
          { "name",                playerName },
          { "userid",              command.SteamID },
          { "reason",              command.Reason },
          { "duration",            humanReadableDuration },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };

        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.mutefailed", banVars);
      }
    }

    private static void UnmutePlayer(Interface.MuteCommand command)
    {
      string userID = command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam";
      string playerName = "";

      if (Utilities.TryGetPlayer(userID, out Player player))
      {
        MuteSystem.ignoreUserID = userID;
        PlayerUnmutingEventArgs unmutingEventArgs = new PlayerUnmutingEventArgs(player.ReferenceHub, Player.Host?.ReferenceHub, false);
        PlayerEvents.OnUnmuting(unmutingEventArgs);
        if (unmutingEventArgs.IsAllowed)
        {
          EmbedMessage embed = new EmbedMessage
          {
            Colour = EmbedMessage.Types.DiscordColour.Red,
            ChannelID = command.ChannelID,
            InteractionID = command.InteractionID
          };

          Dictionary<string, string> banVars = new Dictionary<string, string>
          {
            { "name",                playerName },
            { "userid",              command.SteamID },
            { "discord-displayname", command.DiscordDisplayName },
            { "discord-displayname", command.DiscordDisplayName },
            { "discord-username",    command.DiscordUsername },
            { "discord-userid",      command.DiscordUserID.ToString() },
          };

          SCPDiscord.SendEmbedWithMessageByID(embed, "messages.unmutefailed", banVars);
          return;
        }

        playerName = player.Nickname;
      }
      else
      {
        // If the player is not online, check the steam api instead
        Utilities.TryGetSteamName(userID, out playerName);
      }

      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "name",                playerName },
        { "userid",              command.SteamID },
        { "discord-displayname", command.DiscordDisplayName },
        { "discord-username",    command.DiscordUsername },
        { "discord-userid",      command.DiscordUserID.ToString() }
      };

      if (MuteSystem.UnmutePlayer(ref playerName, userID, command.DiscordUsername))
      {
        EmbedMessage embed = new EmbedMessage
        {
          Colour = EmbedMessage.Types.DiscordColour.Green,
          ChannelID = command.ChannelID,
          InteractionID = command.InteractionID
        };

        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playerunmuted", variables);
      }
      else
      {
        EmbedMessage embed = new EmbedMessage
        {
          Colour = EmbedMessage.Types.DiscordColour.Red,
          ChannelID = command.ChannelID,
          InteractionID = command.InteractionID
        };

        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.unmutefailed", variables);
      }
    }
  }
}