using System;
using System.Collections.Generic;
using System.Linq;
using PluginAPI.Core;
using PluginAPI.Events;
using SCPDiscord.Interface;
using VoiceChat;

namespace SCPDiscord.BotCommands
{
    public class MuteCommand
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
			if (!Utilities.IsPossibleSteamID(command.SteamID))
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

			if (command.Duration.ToLower().Trim().Contains("permanent"))
			{
				endTime = DateTime.MaxValue;
			}
			else
			{
				try
				{
					endTime = Utilities.ParseCompoundDuration(command.Duration.Trim(), ref humanReadableDuration, ref durationSeconds);
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
			}

			string name = "";
			if (!Utilities.GetPlayerName(command.SteamID, ref name))
			{
				name = "Offline player";
			}

			if (command.Reason == "")
			{
				command.Reason = "No reason provided.";
			}

			if (endTime > DateTime.Now)
			{
				MutePlayer(command, name, endTime, humanReadableDuration);
			}
			else
			{
				UnmutePlayer(command, name);
			}
		}

	    private static void MutePlayer(Interface.MuteCommand command, string playerName, DateTime endTime, string humanReadableDuration)
	    {
		    // TODO: Feedback if the request is cancelled by another plugin
		    if (Player.TryGet(command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam", out Player player))
		    {
			    if (!EventManager.ExecuteEvent(new PlayerMutedEvent(player.ReferenceHub, Server.Instance.ReferenceHub, false)))
			    {
				    return;
			    }
		    }
		    VoiceChatMutes.IssueLocalMute(command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam");

		    // TODO: Add to mute file

		    EmbedMessage embed = new EmbedMessage
		    {
			    Colour = EmbedMessage.Types.DiscordColour.Green,
			    ChannelID = command.ChannelID,
			    InteractionID = command.InteractionID
		    };

		    Dictionary<string, string> banVars = new Dictionary<string, string>
		    {
			    { "name",       playerName            },
			    { "steamid",    command.SteamID       },
			    { "reason",     command.Reason        },
			    { "duration",   humanReadableDuration },
			    { "admintag",   command.AdminTag      }
		    };

		    if (endTime == DateTime.MaxValue)
		    {
				SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playermuted", banVars);
		    }
		    else
		    {
			    SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playertempmuted", banVars);
		    }
	    }

	    private static void UnmutePlayer(Interface.MuteCommand command, string playerName)
	    {
		    // TODO: Feedback if the request is cancelled by another plugin
		    if (Player.TryGet(command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam", out Player player))
		    {
			    if (!EventManager.ExecuteEvent(new PlayerMutedEvent(player.ReferenceHub, Server.Instance.ReferenceHub, false)))
			    {
				    return;
			    }
		    }
		    VoiceChatMutes.IssueLocalMute(command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam");

		    // TODO: Modify entry in mute file

		    Dictionary<string, string> banVars = new Dictionary<string, string>
		    {
			    { "name",       playerName            },
			    { "steamid",    command.SteamID       },
			    { "reason",     command.Reason        },
			    { "admintag",   command.AdminTag      }
		    };

		    SCPDiscord.plugin.SendEmbedWithMessageByID(new EmbedMessage
		    {
			    Colour = EmbedMessage.Types.DiscordColour.Green,
			    ChannelID = command.ChannelID,
			    InteractionID = command.InteractionID
		    }, "messages.playerunmuted", banVars);
	    }
    }
}