using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using SCPDiscordPlugin.Utilities;

namespace SCPDiscord;

public static class AdminChat
{
  public static void ReceiveDiscordMessage(Interface.AdminChatDiscordMessage command)
  {
    string sanitizedMessage = Misc.SanitizeRichText(command.Message.Replace("\n", "").Replace("\r", ""));
    string broadcastMessage = $"<color=grey>{command.DiscordUsername}:</color> <color=white>{sanitizedMessage}</color>";

    // Run event to check if anyone wants to stop us
    SendingAdminChatEventArgs ev = new(new DiscordCommandSender(command.DiscordUserID, command.DiscordUsername), broadcastMessage);
    ServerEvents.OnSendingAdminChat(ev);
    if (!ev.IsAllowed)
    {
      return;
    }

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

    // Run event to inform other plugins we have sent the message and trigger our own event handler
    ServerEvents.OnSentAdminChat(new SentAdminChatEventArgs(new DiscordCommandSender(command.DiscordUserID, command.DiscordUsername), broadcastMessage));
  }
}