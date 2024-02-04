using System;
using System.Collections.Generic;
using PluginAPI.Core;
using PluginAPI.Events;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
	// This command is also used for unmuting by setting the duration to 0
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

			if (endTime > DateTime.UtcNow)
			{
				MutePlayer(command, endTime, humanReadableDuration);
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

		    if (Player.TryGet(userID, out Player player))
		    {
			    MuteSystem.ignoreUserID = userID;
			    if (!EventManager.ExecuteEvent(new PlayerMutedEvent(player.ReferenceHub, Server.Instance.ReferenceHub, false)))
			    {
				    EmbedMessage embed = new EmbedMessage
				    {
					    Colour = EmbedMessage.Types.DiscordColour.Red,
					    ChannelID = command.ChannelID,
					    InteractionID = command.InteractionID
				    };

				    Dictionary<string, string> banVars = new Dictionary<string, string>
				    {
					    { "name",     playerName                 },
					    { "userid",   command.SteamID            },
					    { "reason",   command.Reason             },
					    { "duration", humanReadableDuration      },
					    { "admintag", command.AdminTag           },
					    { "adminid",  command.AdminID.ToString() }
				    };

				    SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.mutefailed", banVars);
				    return;
			    }
			    playerName = player.Nickname;
		    }
		    else
		    {
			    // If the player is not online, check the steam api instead
			    Utilities.TryGetSteamName(userID, out playerName);
		    }

		    if (MuteSystem.MutePlayer(ref playerName, userID, command.AdminTag, command.Reason, endTime))
		    {
			    EmbedMessage embed = new EmbedMessage
			    {
				    Colour = EmbedMessage.Types.DiscordColour.Green,
				    ChannelID = command.ChannelID,
				    InteractionID = command.InteractionID
			    };

			    Dictionary<string, string> banVars = new Dictionary<string, string>
			    {
				    { "name",     playerName                 },
				    { "userid",   command.SteamID            },
				    { "reason",   command.Reason             },
				    { "duration", humanReadableDuration      },
				    { "admintag", command.AdminTag           },
				    { "adminid",  command.AdminID.ToString() }
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
				    { "name",     playerName                 },
				    { "userid",   command.SteamID            },
				    { "reason",   command.Reason             },
				    { "duration", humanReadableDuration      },
				    { "admintag", command.AdminTag           },
				    { "adminid",  command.AdminID.ToString() }
			    };

			    SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.mutefailed", banVars);
		    }
	    }

	    private static void UnmutePlayer(Interface.MuteCommand command)
	    {
		    string userID = command.SteamID.EndsWith("@steam") ? command.SteamID : command.SteamID + "@steam";
		    string playerName = "";

		    if (Player.TryGet(userID, out Player player))
		    {
			    MuteSystem.ignoreUserID = userID;
			    if (!EventManager.ExecuteEvent(new PlayerMutedEvent(player.ReferenceHub, Server.Instance.ReferenceHub, false)))
			    {
				    EmbedMessage embed = new EmbedMessage
				    {
					    Colour = EmbedMessage.Types.DiscordColour.Red,
					    ChannelID = command.ChannelID,
					    InteractionID = command.InteractionID
				    };

				    Dictionary<string, string> banVars = new Dictionary<string, string>
				    {
					    { "name",     playerName                 },
					    { "userid",   command.SteamID            },
					    { "admintag", command.AdminTag           },
					    { "adminid",  command.AdminID.ToString() }
				    };

				    SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.unmutefailed", banVars);
				    return;
			    }
			    playerName = player.Nickname;
		    }
		    else
		    {
			    // If the player is not online, check the steam api instead
			    Utilities.TryGetSteamName(userID, out playerName);
		    }

		    if (MuteSystem.UnmutePlayer(ref playerName, userID, command.AdminTag))
		    {
			    EmbedMessage embed = new EmbedMessage
			    {
				    Colour = EmbedMessage.Types.DiscordColour.Green,
				    ChannelID = command.ChannelID,
				    InteractionID = command.InteractionID
			    };

			    Dictionary<string, string> variables = new Dictionary<string, string>
			    {
				    { "name",     playerName       			 },
				    { "userid",   command.SteamID  			 },
				    { "admintag", command.AdminTag 			 },
				    { "adminid",  command.AdminID.ToString() }
			    };

			    SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playerunmuted", variables);
		    }
		    else
		    {
			    EmbedMessage embed = new EmbedMessage
			    {
				    Colour = EmbedMessage.Types.DiscordColour.Red,
				    ChannelID = command.ChannelID,
				    InteractionID = command.InteractionID
			    };

			    Dictionary<string, string> variables = new Dictionary<string, string>
			    {
				    { "name",     playerName       },
				    { "userid",   command.SteamID  },
				    { "admintag", command.AdminTag }
			    };

			    SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.unmutefailed", variables);
		    }
	    }
    }
}