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
    public class RoleCommands
    {
      [JsonProperty("ids")]
      public ulong[] discordIDs = [];

      [JsonProperty("commands")]
      public string[] commands = [];

      [JsonProperty("remoteadmin-rank")]
      public string remoteAdminRank = "";

      [JsonProperty("reserved-slot")]
      public bool reservedSlot = false;

      [JsonProperty("sub-roles")]
      public Dictionary<string, RoleCommands> subRoles = new();
    }

    public class RolesyncResult
    {
      public bool reservedSlot = false;
      public string remoteAdminRank = "";
      public List<string> commands = [];
    }

    internal static Dictionary<string, RoleCommands> config = [];

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

      if (!File.Exists(Config.GetRoleSyncPath()))
      {
        Logger.Info("Rolesync file \"" + Config.GetRoleSyncPath() + "\" does not exist, creating...");
        File.WriteAllText(Config.GetRoleSyncPath(), "{}");
      }

      try
      {
        syncedPlayers = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(File.ReadAllText(Config.GetRoleSyncPath()));
      }
      catch (Exception)
      {
        try
        {
          Logger.Warn("Could not read rolesync file \"" + Config.GetRoleSyncPath() + "\", attempting to convert file from old format...");
          syncedPlayers = JArray.Parse(File.ReadAllText(Config.GetRoleSyncPath())).ToDictionary(
            k => ((JObject)k).Properties().First().Name,
            v => v.Values().First().Value<ulong>());
          File.WriteAllText(Config.GetRoleSyncPath(), JsonConvert.SerializeObject(syncedPlayers, Formatting.Indented));
        }
        catch (Exception e)
        {
          Logger.Error("Could not read rolesync file \"" + Config.GetRoleSyncPath() + "\", check the file formatting and try again.");
          Logger.Debug("[RS] Error: " + e);
          throw;
        }
      }

      fileWatcher = new Utilities.FileWatcher(Config.GetRolesyncDir(), "rolesync.json", Reload);
      Logger.Debug("[RS] Reloaded \"" + Config.GetRoleSyncPath() + "\".");
    }

    private static void SavePlayers()
    {
      File.WriteAllText(Config.GetRoleSyncPath(), JsonConvert.SerializeObject(syncedPlayers, Formatting.Indented));
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
          Logger.Debug("[RS] User ID '" + player.UserId + "' is not in rolesync list.");
          return;
        }

        MessageWrapper message = new()
        {
          UserQuery = new UserQuery
          {
            SteamIDOrIP = player.UserId,
            DiscordID = syncedPlayer
          }
        };

        Network.QueueMessage(message);
      }
      else
      {
        if (!syncedPlayers.TryGetValue(player.IpAddress, out ulong syncedPlayer))
        {
          Logger.Debug("[RS] IP '" + player.IpAddress + "' is not in rolesync list.");
          return;
        }

        MessageWrapper message = new()
        {
          UserQuery = new UserQuery
          {
            SteamIDOrIP = player.IpAddress,
            DiscordID = syncedPlayer
          }
        };

        Network.QueueMessage(message);
      }
    }

    private static bool TryGetMatchingPlayers(string steamIDOrIP, out List<Player> matchingPlayers)
    {
      // For online servers this should always be one player, but for offline servers it may match several
      matchingPlayers = new List<Player>();
      try
      {
        Logger.Debug("[RS] Looking for player with SteamID/IP: " + steamIDOrIP);
        foreach (Player player in Player.ReadyList)
        {
          Logger.Debug($"[RS] Trying player \"{player.PlayerId}\": SteamID {player.UserId} IP {player.IpAddress}");
          if (player.UserId == steamIDOrIP)
          {
            Logger.Debug("[RS] Matching SteamID found");
            matchingPlayers.Add(player);
          }
          else if (player.IpAddress == steamIDOrIP)
          {
            Logger.Debug("[RS] Matching IP found");
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

    private static void GetRolesyncActions(RoleCommands roleCommands, UserInfo userInfo, Player player, out RolesyncResult result)
    {
      result = new RolesyncResult();
      foreach (KeyValuePair<string, RoleCommands> subRole in roleCommands.subRoles)
      {
        if (subRole.Value.discordIDs.Length == 0 || subRole.Value.discordIDs.Any(id => userInfo.RoleIDs.Contains(id)))
        {
          Logger.Debug($"[RS] Player {player.DisplayName} has sub-role \"{subRole.Key}\", entering it...");
          GetRolesyncActions(subRole.Value, userInfo, player, out result);

          // We should only run rolesync on the first sub-role as it is assumed to be the highest ranked role
          break;
        }
        Logger.Debug($"[RS] Player {player.DisplayName} does not have sub-role \"{subRole.Key}\".");
      }

      // The player gets a reserved slot as long as any role in the chain allows it
      if (roleCommands.reservedSlot)
      {
        result.reservedSlot = true;
      }

      // Only set the role if a point further down the tree hasn't already done so
      if (!string.IsNullOrWhiteSpace(roleCommands.remoteAdminRank) && string.IsNullOrWhiteSpace(result.remoteAdminRank))
      {
        result.remoteAdminRank = roleCommands.remoteAdminRank;
      }

      if (roleCommands.commands != null)
      {
        result.commands.AddRange(roleCommands.commands);
      }
    }

    private static void RunRoleCommandsCompat(UserInfo userInfo, Player player, Dictionary<string, string> variables)
    {
      foreach (KeyValuePair<ulong, string[]> keyValuePair in configCompat)
      {
        if (!userInfo.RoleIDs.Contains(keyValuePair.Key))
        {
          Logger.Debug("[RS] User does not have discord role " + keyValuePair.Key);
          continue;
        }

        Logger.Debug($"[RS] User has discord role {keyValuePair.Key}, scheduling configured commands...");

        foreach (string unparsedCommand in keyValuePair.Value)
        {
          string command = unparsedCommand;
          foreach (KeyValuePair<string, string> variable in variables)
          {
            command = command.Replace("<var:" + variable.Key + ">", variable.Value);
          }

          Logger.Debug("[RS] Running rolesync command: " + command);
          SCPDiscord.plugin.sync.ScheduleRoleSyncCommand(command);
        }

        Logger.Info($"Synced \"{player.Nickname}\" ({userInfo.SteamIDOrIP}) with Discord role id {keyValuePair.Key}");
        return;
      }
    }

    public static void ExecuteRoleSync(Player player, UserInfo userInfo, Dictionary<string, string> variables)
    {
      Logger.Info($"Running rolesync for \"{player.Nickname}\" ({userInfo.SteamIDOrIP}).");
      RolesyncResult result = new();
      foreach (KeyValuePair<string, RoleCommands> role in config)
      {
        if (role.Value.discordIDs.Length == 0
            || role.Value.discordIDs.Any(id => userInfo.RoleIDs.Contains(id))
            || role.Value.discordIDs.Any(id => userInfo.DiscordUserID == id))
        {
          Logger.Debug($"[RS] Player {player.DisplayName} has role \"{role.Key}\", entering it...");
          GetRolesyncActions(role.Value, userInfo, player, out result);
          break;
        }
        Logger.Debug($"[RS] Player {player.DisplayName} does not have role \"{role.Key}\".");
      }

      Logger.Debug("[RS] Done collecting rolesync actions, executing them now.");

      if (result.reservedSlot)
      {
        Logger.Debug("[RS] Player has reserved slot, granting it...");
        Utilities.GrantReservedSlot(player.UserId, userInfo.DiscordUsername);
      }
      else
      {
        Logger.Debug("[RS] Player does not have reserved slot, revoking it...");
        Utilities.RevokeReservedSlot(player.UserId);
      }

      if (!string.IsNullOrWhiteSpace(result.remoteAdminRank))
      {
        Logger.Debug($"[RS] Player has remote admin rank '{result.remoteAdminRank}', setting it...");
        player.SetRank(null, null, result.remoteAdminRank);
      }

      foreach (string unparsedCommand in result.commands)
      {
        string command = unparsedCommand;
        foreach (KeyValuePair<string, string> variable in variables)
        {
          command = command.Replace("<var:" + variable.Key + ">", variable.Value);
        }

        Logger.Debug("[RS] Running rolesync command: " + command);
        SCPDiscord.plugin.sync.ScheduleRoleSyncCommand(command);
      }
    }

    public static void ReceiveQueryResponse(UserInfo userInfo)
    {
      Thread.Sleep(1000);

      if (!TryGetMatchingPlayers(userInfo.SteamIDOrIP, out List<Player> matchingPlayers))
      {
        return;
      }

      foreach (Player player in matchingPlayers)
      {
        Dictionary<string, string> variables = new()
        {
          { "discord-displayname", userInfo.DiscordDisplayName },
          { "discord-username", userInfo.DiscordUsername },
          { "discord-userid", userInfo.DiscordUserID.ToString() }
        };
        variables.AddPlayerVariables(player, "player");

        // Compatibility mode (pre 3.4.0)
        if (compatibilityMode)
        {
          Logger.Info($"Running rolesync in compatibility mode for \"{player.Nickname}\" ({userInfo.SteamIDOrIP}).");
          RunRoleCommandsCompat(userInfo, player, variables);
        }
        else
        {
          ExecuteRoleSync(player, userInfo, variables);
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

    // TODO: Should be a try get
    public static bool IsPlayerSynced(string userID, out ulong discordID)
    {
      return syncedPlayers.TryGetValue(userID.EndsWith("@steam") ? userID : userID + "@steam", out discordID);
    }

    // TODO: Should be a try get
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
