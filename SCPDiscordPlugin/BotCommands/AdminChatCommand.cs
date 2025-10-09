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
			EmbedMessage embed = new EmbedMessage
			{
				Colour = EmbedMessage.Types.DiscordColour.Green,
				ChannelID = command.ChannelID,
				InteractionID = command.InteractionID
			};

			foreach (var staff in ReferenceHub.AllHubs)
			{
				if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.AdminChat))
					continue;
				string message = $"<color=white>{command.DiscordUsername}: {command.Message}</color>";
				Broadcast.Singleton.TargetAddElement(staff.connectionToClient, message, 5, Broadcast.BroadcastFlags.AdminChat);
				/**
				* 0 - Host.NetworkId
				* ! - Separator
				* @@ - Make message silent without red background
				*/
				//Server.SendAdminChatMessage
				staff.encryptedChannelManager.TrySendMessageToClient($"0![DISCORD] {message}", EncryptedChannelManager.EncryptedChannel.AdminChat);
			}

			// Invoke event to trigger that admin chat message was sent
			ServerEvents.OnSentAdminChat(new SentAdminChatEventArgs(new DiscordCommandSender(command.DiscordUserID, command.DiscordUsername), command.Message));

			SCPDiscord.SendEmbedWithMessageByID(embed, "messages.adminchatsent");
		}
	}
}
