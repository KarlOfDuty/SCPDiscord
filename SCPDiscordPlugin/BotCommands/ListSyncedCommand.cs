using System;
using System.Collections.Generic;
using PluginAPI.Core;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
    public class ListSyncedCommand
    {
	    public static void Execute(Interface.ListSyncedCommand command)
	    {
		    string messageType = command.ListAll ? "all" : "online-only";
            List<string> listItems = command.ListAll
	                                 ? GetAllSyncedPlayers(command.ChannelID)
	                                 : GetOnlineSyncedPlayers(command.ChannelID);

		    if (listItems.Count == 0)
            {
            	EmbedMessage embed = new EmbedMessage
            	{
            		Title = Language.GetProcessedMessage("messages.list.synced.title." + messageType, new Dictionary<string, string>
            		{
            			{ "players",       Math.Max(0, Player.Count).ToString() },
            			{ "syncedplayers", listItems.Count.ToString() },
            			{ "maxplayers",    Server.MaxPlayers.ToString() },
			            { "page",          "1" },
			            { "pages",         "1" }
            		}),
            		Description = Language.GetProcessedMessage("messages.list.synced.row." + messageType + ".empty", new Dictionary<string, string>()),
            		Colour = EmbedMessage.Types.DiscordColour.Red,
            		ChannelID = command.ChannelID,
            		InteractionID = command.InteractionID
            	};
            	SCPDiscord.plugin.SendEmbedByID(embed);
            	return;
            }

            List<EmbedMessage> embeds = new List<EmbedMessage>();

            int pageNum = 0;
            LinkedList<string> pages = Utilities.ParseListIntoMessages(listItems);
            foreach (string message in pages)
            {
	            ++pageNum;
            	embeds.Add(new EmbedMessage
            	{
            		Title = Language.GetProcessedMessage("messages.list.synced.title." + messageType, new Dictionary<string, string>
            		{
            			{ "players",       Math.Max(0, Player.Count).ToString() },
            			{ "syncedplayers", listItems.Count.ToString()           },
            			{ "maxplayers",    Server.MaxPlayers.ToString()         },
			            { "page",          pageNum.ToString()                   },
			            { "pages",         pages.Count.ToString()               }
            		}),
            		Colour = EmbedMessage.Types.DiscordColour.Cyan,
            		Description = message
            	});
            }

            PaginatedMessage response = new PaginatedMessage
            {
            	ChannelID = command.ChannelID,
            	UserID = command.UserID,
            	InteractionID = command.InteractionID
            };
            response.Pages.Add(embeds);

            NetworkSystem.QueueMessage(new MessageWrapper { PaginatedMessage = response });
	    }

	    private static List<string> GetAllSyncedPlayers(ulong channelID)
	    {
		    List<string> rows = new List<string>();
		    foreach (KeyValuePair<string,ulong> syncedPlayer in RoleSync.GetSyncedPlayers())
		    {
			    if (!Utilities.TryGetPlayerName(syncedPlayer.Key, out string name))
			    {
				    if (!Utilities.TryGetSteamName(syncedPlayer.Key, out name))
				    {
					    name = "Unknown Player";
				    }
			    }

			    Dictionary<string, string> variables = new Dictionary<string, string>
			    {
				    { "userid", syncedPlayer.Key.Replace("@steam", "") },
				    { "name", name },
				    { "discordid", syncedPlayer.Value.ToString() },
				    { "discordmention", "<@" + syncedPlayer.Value + ">" }
			    };
			    string row = Language.GetProcessedMessage("messages.list.synced.row.all.default", variables);

			    Language.RunFiltersManually(channelID, null, syncedPlayer.Key, "<hidden>", ref row);

			    rows.Add(row);
		    }
		    return rows;
	    }

	    private static List<string> GetOnlineSyncedPlayers(ulong channelID)
	    {
		    List<string> rows = new List<string>();
			foreach (Player player in Player.GetPlayers())
			{
				if (!RoleSync.IsPlayerSynced(player.UserId, out ulong discordID)) continue;

				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "discordid", discordID.ToString() },
					{ "discordmention", "<@" + discordID + ">" }
				};
				variables.AddPlayerVariables(player, "player");
				string row = Language.GetProcessedMessage("messages.list.synced.row.online-only.default", variables);

				Language.RunFilters(channelID, player, ref row);

				rows.Add(row);
			}

			return rows;
		}
    }
}