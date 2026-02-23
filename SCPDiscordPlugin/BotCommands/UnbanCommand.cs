using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
  public static class UnbanCommand
  {
    private const long SCPSL_RELEASE_DATE = 636501024000000000; // DateTime.Parse("2017-12-29").Ticks;

    public static void Execute(Interface.UnbanCommand command)
    {
      EmbedMessage embed = new()
      {
        Colour = EmbedMessage.Types.DiscordColour.Red,
        ChannelID = command.ChannelID,
        InteractionID = command.InteractionID
      };

      // Perform very basic SteamID and ip validation.
      if (!Utilities.IsPossibleSteamID(command.SteamIDOrIP, out ulong _) && !IPAddress.TryParse(command.SteamIDOrIP, out IPAddress _))
      {
        Dictionary<string, string> variables = new()
        {
          { "steamidorip",         command.SteamIDOrIP },
          { "discord-displayname", command.DiscordDisplayName },
          { "discord-username",    command.DiscordUsername },
          { "discord-userid",      command.DiscordUserID.ToString() },
        };
        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.invalidsteamidorip", variables);
        return;
      }

      // Read ip bans if the file exists
      List<string> ipBans = [];
      if (File.Exists(Config.GetIPBansFile()))
      {
        ipBans = File.ReadAllLines(Config.GetIPBansFile()).ToList();
      }
      else
      {
        Logger.Warn(Config.GetIPBansFile() + " does not exist, could not check it for banned players.");
      }

      // Read steam id bans if the file exists
      List<string> steamIDBans = [];
      if (File.Exists(Config.GetUserIDBansFile()))
      {
        steamIDBans = File.ReadAllLines(Config.GetUserIDBansFile()).ToList();
      }
      else
      {
        Logger.Warn(Config.GetUserIDBansFile() + " does not exist, could not check it for banned players.");
      }

      // Get all ban entries to be removed. (Splits the string and only checks the steam id and ip of the banned players instead of entire strings)
      List<string> matchingIPBans = ipBans.FindAll(s => s.Split(';').ElementAtOrDefault(1)?.Contains(command.SteamIDOrIP) ?? false);
      List<string> matchingSteamIDBans = steamIDBans.FindAll(s => s.Split(';').ElementAtOrDefault(1)?.Contains(command.SteamIDOrIP) ?? false);

      // Delete the entries from the original containers now that there is a backup of them
      ipBans.RemoveAll(s => matchingIPBans.Any(str => str == s));
      steamIDBans.RemoveAll(s => matchingSteamIDBans.Any(str => str == s));

      // Check if either ban file has a ban with a time stamp matching the one removed and remove it too as
      // most servers create both a steamid-ban entry and an ip-ban entry.
      foreach (string row in matchingIPBans)
      {
        string timestamp = row.Split(';').Last();
        if (long.TryParse(timestamp, out long timestampTicks) && timestampTicks > SCPSL_RELEASE_DATE)
        {
          steamIDBans.RemoveAll(s => s.Contains(timestamp));
        }
      }

      foreach (string row in matchingSteamIDBans)
      {
        string timestamp = row.Split(';').Last();
        if (ulong.TryParse(timestamp, out ulong timestampTicks) && timestampTicks > SCPSL_RELEASE_DATE)
        {
          ipBans.RemoveAll(s => s.Contains(timestamp));
        }
      }

      // Save the edited ban files if they exist
      if (File.Exists(Config.GetIPBansFile()))
      {
        File.WriteAllLines(Config.GetIPBansFile(), ipBans);
      }

      if (File.Exists(Config.GetUserIDBansFile()))
      {
        File.WriteAllLines(Config.GetUserIDBansFile(), steamIDBans);
      }

      BanHandler.ValidateBans();

      // Send response message to Discord
      Dictionary<string, string> unbanVars = new Dictionary<string, string>
      {
        { "steamidorip",         command.SteamIDOrIP },
        { "discord-displayname", command.DiscordDisplayName },
        { "discord-username",    command.DiscordUsername },
        { "discord-userid",      command.DiscordUserID.ToString() },
      };
      embed.Colour = EmbedMessage.Types.DiscordColour.Green;
      SCPDiscord.SendEmbedWithMessageByID(embed, "messages.playerunbanned", unbanVars);
    }
  }
}