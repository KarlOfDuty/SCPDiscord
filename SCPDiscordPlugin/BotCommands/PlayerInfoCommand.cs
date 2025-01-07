using System;
using System.Collections.Generic;
using PluginAPI.Core;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
  public static class PlayerInfoCommand
  {
    public static void Execute(Interface.PlayerInfoCommand command)
    {
      EmbedMessage embed = new EmbedMessage
      {
        Colour = EmbedMessage.Types.DiscordColour.Green,
        ChannelID = command.ChannelID,
        InteractionID = command.InteractionID
      };

      command.SteamID = command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam";

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
        embed.Colour = EmbedMessage.Types.DiscordColour.Red;
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.invalidsteamid", vars);
        return;
      }

      bool isSynced = RoleSync.IsPlayerSynced(command.SteamID, out ulong discordID);

      string muteStatus = "No";
      if (MuteSystem.IsMuted(command.SteamID, out DateTime muteEnd, out string muteReason))
      {
        if (muteEnd == DateTime.MaxValue)
        {
          muteStatus = "Permanently";
        }
        else
        {
          muteStatus = "Ends at " + muteEnd.ToString("yyyy-MM-dd HH:mm");
        }
      }

      if (Utilities.TryGetPlayer(command.SteamID, out Player player))
      {
        Dictionary<string, string> vars = new Dictionary<string, string>
        {
          { "discordid",           discordID.ToString() },
          { "discordmention",      "<@" + discordID + ">" },
          { "issynced",            isSynced.ToString() },
          { "mutestatus",          muteStatus },
          { "mutereason",          muteReason },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };
        vars.AddPlayerVariables(player, "player");
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playerinfo.online", vars);
      }
      else
      {
        if (!Utilities.TryGetSteamName(command.SteamID, out string name))
        {
          name = "Unknown";
        }

        string banStatus = "No";
        string banReason = "";
        KeyValuePair<BanDetails, BanDetails> pair = BanHandler.QueryBan(command.SteamID, null);
        if (pair.Key != null && new DateTime(pair.Key.Expires) > DateTime.UtcNow)
        {
          banStatus = "Ends at " + new DateTime(pair.Key.Expires).ToString("yyyy-MM-dd HH:mm");
          banReason = pair.Key.Reason;
        }

        Dictionary<string, string> vars = new Dictionary<string, string>
        {
          { "name",                name },
          { "userid",              command.SteamID.Replace("@steam", "") },
          { "discordid",           discordID.ToString() },
          { "discordmention",      "<@" + discordID + ">" },
          { "issynced",            isSynced.ToString() },
          { "mutestatus",          muteStatus },
          { "mutereason",          muteReason },
          { "banstatus",           banStatus },
          { "banreason",           banReason },
          { "playtimehours",       PlayTime.GetHours(command.SteamID) },
          { "hasreservedslot",     ReservedSlots.HasReservedSlot(command.SteamID).ToString() },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playerinfo.offline", vars);
      }
    }
  }
}
