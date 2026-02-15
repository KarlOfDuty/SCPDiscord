using Newtonsoft.Json.Linq;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CentralAuth;
using LabApi.Features.Wrappers;
using Newtonsoft.Json;

namespace SCPDiscord
{
  public static class RoleSync
  {
    public struct RoleCommands
    {
      public ulong roleID;
      public string[] commands;
      public RoleCommands[] subRoles;
    }

    internal static List<RoleCommands> config = [];

    // Old rolesync config format for backward compatibility (pre 3.4.0)
    internal static bool compatibilityMode = false;
    internal static Dictionary<ulong, string[]> configCompat = new();

    private static Dictionary<string, ulong> syncedPlayers = new();

    private static Utilities.FileWatcher fileWatcher;

    public static void Reload()
    {
      fileWatcher?.Dispose();

      if (!Directory.Exists(Config.GetRolesyncDir()))
      {
        Directory.CreateDirectory(Config.GetRolesyncDir());
      }

      if (!File.Exists(Config.GetRolesyncPath()))
      {
        Logger.Info("Rolesync file \"" + Config.GetRolesyncPath() + "\" does not exist, creating...");
        File.WriteAllText(Config.GetRolesyncPath(), "{}");
      }

      try
      {
        syncedPlayers = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(File.ReadAllText(Config.GetRolesyncPath()));
      }
      catch (Exception)
      {
        try
        {
          Logger.Warn("Could not read rolesync file \"" + Config.GetRolesyncPath() + "\", attempting to convert file from old format...");
          syncedPlayers = JArray.Parse(File.ReadAllText(Config.GetRolesyncPath())).ToDictionary(
            k => ((JObject)k).Properties().First().Name,
            v => v.Values().First().Value<ulong>());
          File.WriteAllText(Config.GetRolesyncPath(), JsonConvert.SerializeObject(syncedPlayers, Formatting.Indented));
        }
        catch (Exception e)
        {
          Logger.Error("Could not read rolesync file \"" + Config.GetRolesyncPath() + "\", check the file formatting and try again.");
          Logger.Debug("Error: " + e);
          throw;
        }
      }

      fileWatcher = new Utilities.FileWatcher(Config.GetRolesyncDir(), "rolesync.json", Reload);
      Logger.Debug("Reloaded \"" + Config.GetRolesyncPath() + "\".");
    }

    private static void SavePlayers()
    {
      File.WriteAllText(Config.GetRolesyncPath(), JsonConvert.SerializeObject(syncedPlayers, Formatting.Indented));
    }

    public static void SendRoleQuery(Player player)
    {
      if (player?.PlayerId == null || player?.IpAddress == null)
      {
        Logger.Warn("Unable to sync player, player object was null.");
        return;
      }

      if (PlayerAuthenticationManager.OnlineMode)
      {
        if (!syncedPlayers.TryGetValue(player.UserId, out ulong syncedPlayer))
        {
          Logger.Debug("User ID '" + player.UserId + "' is not in rolesync list.");
          return;
        }

        MessageWrapper message = new MessageWrapper
        {
          UserQuery = new UserQuery
          {
            SteamIDOrIP = player.UserId,
            DiscordID = syncedPlayer
          }
        };

        NetworkSystem.QueueMessage(message);
      }
      else
      {
        if (!syncedPlayers.TryGetValue(player.IpAddress, out ulong syncedPlayer))
        {
          Logger.Debug("IP '" + player.IpAddress + "' is not in rolesync list.");
          return;
        }

        MessageWrapper message = new MessageWrapper
        {
          UserQuery = new UserQuery
          {
            SteamIDOrIP = player.IpAddress,
            DiscordID = syncedPlayer
          }
        };

        NetworkSystem.QueueMessage(message);
      }
    }

    private static bool TryGetMatchingPlayers(string steamIDOrIP, out List<Player> matchingPlayers)
    {
      // For online servers this should always be one player, but for offline servers it may match several
      matchingPlayers = new List<Player>();
      try
      {
        Logger.Debug("Looking for player with SteamID/IP: " + steamIDOrIP);
        foreach (Player player in Player.ReadyList)
        {
          Logger.Debug("Player " + player.PlayerId + ": SteamID " + player.UserId + " IP " + player.IpAddress);
          if (player.UserId == steamIDOrIP)
          {
            Logger.Debug("Matching SteamID found");
            matchingPlayers.Add(player);
          }
          else if (player.IpAddress == steamIDOrIP)
          {
            Logger.Debug("Matching IP found");
            matchingPlayers.Add(player);
          }
        }
      }
      catch (Exception e)
      {
        Logger.Error("Error getting player for RoleSync: " + e);
        return false;
      }

      if (matchingPlayers.Count == 0)
      {
        Logger.Error("Could not get player for rolesync, did they disconnect immediately?");
        return false;
      }

      return true;
    }

    public static void ReceiveQueryResponse(UserInfo userInfo)
    {
      Thread.Sleep(1000);

      if (!TryGetMatchingPlayers(userInfo.SteamIDOrIP, out List<Player> matchingPlayers))
      {
        return;
      }

      // TODO: Add new rolesync system here

      foreach (Player player in matchingPlayers)
      {
        foreach (KeyValuePair<ulong, string[]> keyValuePair in roleSyncConfCompat)
        {
          if (!userInfo.RoleIDs.Contains(keyValuePair.Key))
          {
            Logger.Debug("User has discord role " + keyValuePair.Key + ": " + userInfo.RoleIDs.Contains(keyValuePair.Key));
            continue;
          }

          Logger.Debug("User has discord role " + keyValuePair.Key + ", scheduling configured commands...");

          Dictionary<string, string> variables = new()
          {
            { "discord-displayname", userInfo.DiscordDisplayName   },
            { "discord-username",    userInfo.DiscordUsername      },
            { "discord-userid",      userInfo.DiscordUserID.ToString() },
            // Old names for backwards compatibility (pre 3.2.1)
            { "discorddisplayname", userInfo.DiscordDisplayName   },
            { "discordusername",    userInfo.DiscordUsername      },
            { "discordid",      userInfo.DiscordUserID.ToString() }
          };
          variables.AddPlayerVariables(player, "player");

          foreach (string unparsedCommand in keyValuePair.Value)
          {
            string command = unparsedCommand;
            foreach (KeyValuePair<string, string> variable in variables)
            {
              command = command.Replace("<var:" + variable.Key + ">", variable.Value);
            }

            Logger.Debug("Running rolesync command: " + command);
            SCPDiscord.plugin.sync.ScheduleRoleSyncCommand(command);
          }

          Logger.Info("Synced " + player.Nickname + " (" + userInfo.SteamIDOrIP + ") with Discord role id " + keyValuePair.Key);
          return;
        }
      }
    }

    public static EmbedMessage AddPlayer(SyncRoleCommand command)
    {
      EmbedMessage msg = new()
      {
        Colour = EmbedMessage.Types.DiscordColour.Red,
        ChannelID = command.ChannelID,
        Description = "",
        InteractionID = command.InteractionID
      };

      if (PlayerAuthenticationManager.OnlineMode)
      {
        if (syncedPlayers.ContainsKey(command.SteamIDOrIP + "@steam"))
        {
          msg.Description = "SteamID is already linked to a Discord account. You will have to remove it first.";
          return msg;
        }

        if (syncedPlayers.ContainsValue(command.DiscordUserID))
        {
          msg.Description = "Discord user ID is already linked to a Steam account. You will have to remove it first.";
          return msg;
        }

        if (!Utilities.TryGetSteamName(command.SteamIDOrIP, out string _))
        {
          msg.Description = "Could not find a user in the Steam API using that ID.";
          return msg;
        }

        syncedPlayers.Add(command.SteamIDOrIP + "@steam", command.DiscordUserID);
        SavePlayers();
        msg.Colour = EmbedMessage.Types.DiscordColour.Green;
        msg.Description = "Successfully linked accounts.";
        return msg;
      }

      if (syncedPlayers.ContainsKey(command.SteamIDOrIP))
      {
        msg.Description = "IP is already linked to a Discord account. You will have to remove it first.";
        return msg;
      }

      if (syncedPlayers.ContainsValue(command.DiscordUserID))
      {
        msg.Description = "Discord user ID is already linked to an IP. You will have to remove it first.";
        return msg;
      }

      syncedPlayers.Add(command.SteamIDOrIP, command.DiscordUserID);
      SavePlayers();
      msg.Colour = EmbedMessage.Types.DiscordColour.Green;
      msg.Description = "Successfully linked IP.";
      return msg;
    }

    public static EmbedMessage RemovePlayer(UnsyncRoleCommand command)
    {
      if (!syncedPlayers.ContainsValue(command.DiscordUserID))
      {
        return new EmbedMessage
        {
          Colour = EmbedMessage.Types.DiscordColour.Red,
          ChannelID = command.ChannelID,
          Description = "Discord user ID is not linked to a Steam account or IP",
          InteractionID = command.InteractionID
        };
      }

      KeyValuePair<string, ulong> player = syncedPlayers.First(kvp => kvp.Value == command.DiscordUserID);
      syncedPlayers.Remove(player.Key);
      SavePlayers();
      return new EmbedMessage
      {
        Colour = EmbedMessage.Types.DiscordColour.Green,
        ChannelID = command.ChannelID,
        Description = "Discord user ID link has been removed.",
        InteractionID = command.InteractionID
      };
    }

    public static string RemovePlayerLocally(ulong discordID)
    {
      if (!syncedPlayers.ContainsValue(discordID))
      {
        return "Discord user ID is not linked to a Steam account or IP";
      }

      KeyValuePair<string, ulong> player = syncedPlayers.First(kvp => kvp.Value == discordID);
      syncedPlayers.Remove(player.Key);
      SavePlayers();
      return "Discord user ID link has been removed.";
    }

    public static bool IsPlayerSynced(string userID, out ulong discordID)
    {
      return syncedPlayers.TryGetValue(userID.EndsWith("@steam") ? userID : userID + "@steam", out discordID);
    }

    public static bool IsPlayerSynced(ulong discordID, out string userID)
    {
      return syncedPlayers.TryGetFirstKey(discordID, out userID);
    }

    public static Dictionary<string, ulong> GetSyncedPlayers()
    {
      return syncedPlayers;
    }
  }
}
