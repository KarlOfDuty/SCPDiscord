using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using SCPDiscord.Interface;
using SCPDiscordPlugin.Helpers;

namespace SCPDiscord.BotCommands
{
  public static class AdminChatCommand
  {
    public static void Execute(Interface.AdminChatCommand command)
    {
      EmbedMessage embed = new()
      {
        Colour = EmbedMessage.Types.DiscordColour.Green,
        ChannelID = command.ChannelID,
        InteractionID = command.InteractionID
      };

      // TODO: Have to include user roles in this so a role colour can be determined, should probably cache the colour though
      string username = $"<color=white>{command.DiscordUsername}</color>";
      if (Utilities.TryGetOfflineRankByDiscordID(command.DiscordUserID, out UserGroup rank))
      {
        username = $"<color={rank.BadgeColor}>{command.DiscordUsername}</color>";
      }

      string sanitizedMessage = Misc.SanitizeRichText(command.Message.Replace("\n", "").Replace("\r", ""));
      string broadcastMessage = $"{username}: <color=white>{sanitizedMessage}</color>";

      foreach (ReferenceHub player in ReferenceHub.AllHubs)
      {
        // Ignore the server and players without admin chat permissions
        if (player.isLocalPlayer || !PermissionsHandler.IsPermitted(player.serverRoles.Permissions, PlayerPermissions.AdminChat))
        {
          continue;
        }

        // Send broadcast message to this player
        Broadcast.Singleton.TargetAddElement(player.connectionToClient, broadcastMessage, 5, Broadcast.BroadcastFlags.AdminChat);

        // Update this player's chat history in the admin panel.
        // Chat history entries use the format <network id>!<message>.
        player.encryptedChannelManager.TrySendMessageToClient($"0![<color=blue>DISCORD</color>] {broadcastMessage}", EncryptedChannelManager.EncryptedChannel.AdminChat);
      }

      ServerEvents.OnSentAdminChat(new SentAdminChatEventArgs(new DiscordCommandSender(command.DiscordUserID, command.DiscordUsername), command.Message));

      SCPDiscord.SendEmbedWithMessageByID(embed, "messages.adminchatsent");
    }
  }
}
