using System;
using System.Collections.Generic;
using System.Linq;
using PluginAPI.Core;
using PluginAPI.Events;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
    public class BanCommand
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
					{ "steamid", command.SteamID }
				};
				SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.invalidsteamid", variables);
				return;
			}

			// Create duration timestamp.
			string humanReadableDuration = "";
			long durationSeconds = 0;
			long issuanceTime = DateTime.UtcNow.Ticks;
			DateTime endTime;
			try
			{
				endTime = Utilities.ParseCompoundDuration(command.Duration, ref humanReadableDuration, ref durationSeconds);
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
				SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.invalidduration", variables);
				return;
			}

			if (!Utilities.TryGetPlayerName(command.SteamID, out string name))
			{
				if (!Utilities.TryGetSteamName(command.SteamID, out name))
				{
					name = "Offline player";
				}
			}

			//Semicolons are separators in the ban file so cannot be part of strings
			name = name.Replace(";", "");
			command.Reason = command.Reason.Replace(";", "");

			if (command.Reason == "")
			{
				command.Reason = "No reason provided.";
			}

			// TODO: Feedback if the request is cancelled by another plugin

			// Send player banned event if player is online, and add ipban
			if (Player.TryGet(command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam", out Player player))
			{
				PlayerBannedEvent eventArgs = new PlayerBannedEvent(player.ReferenceHub, Server.Instance.ReferenceHub, command.Reason, durationSeconds);
				if (!EventManager.ExecuteEvent<bool>(eventArgs))
				{
					return;
				}
				BanHandler.IssueBan(new BanDetails()
				{
					OriginalName = name,
					Id = player.ReferenceHub.connectionToClient.address,
					IssuanceTime = issuanceTime,
					Expires = endTime.Ticks,
					Reason = command.Reason,
					Issuer = command.AdminTag
				}, BanHandler.BanType.IP);
				ServerConsole.Disconnect(player.ReferenceHub.gameObject, "You have been banned. Reason: " + command.Reason);
			}

			BanHandler.IssueBan(new BanDetails()
			{
				OriginalName = name,
				Id = (command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam"),
				IssuanceTime = issuanceTime,
				Expires = endTime.Ticks,
				Reason = command.Reason,
				Issuer = command.AdminTag
			}, BanHandler.BanType.UserId);

			BanHandler.ValidateBans();

			Dictionary<string, string> banVars = new Dictionary<string, string>
			{
				{ "name",       name                   },
				{ "steamid",    command.SteamID        },
				{ "reason",     command.Reason         },
				{ "duration",   humanReadableDuration  },
				{ "admintag",   command.AdminTag       }
			};

			embed.Colour = EmbedMessage.Types.DiscordColour.Green;
			SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playerbanned", banVars);
		}
    }
}