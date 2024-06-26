using System;
using System.Collections.Generic;
using PluginAPI.Core;
using SCPDiscord.Interface;
namespace SCPDiscord.BotCommands
{
    public static class ListRankedCommand
    {
        public static void Execute(Interface.ListRankedCommand command)
		{
			Logger.Debug("Listranked command called by " + command.UserID + " in " + command.ChannelID + ". Interaction: " + command.InteractionID + ")");

			List<string> listItems = new List<string>();
			foreach (Player player in Player.GetPlayers())
			{
				if (!player.TryGetRank(out string _))
				{
					continue;
				}

				Dictionary<string, string> variables = new Dictionary<string, string> {};
				variables.AddPlayerVariables(player, "player");
				string row = Language.GetProcessedMessage("messages.list.ranked.row.default", variables);

				listItems.Add(Language.RunFilters(command.ChannelID, player, row));
			}

			if (listItems.Count == 0)
			{
				EmbedMessage embed = new EmbedMessage
				{
					Title = Language.GetProcessedMessage("messages.list.ranked.title", new Dictionary<string, string>
					{
						{ "players",       Math.Max(0, Player.Count).ToString() },
						{ "rankedplayers", listItems.Count.ToString() },
						{ "maxplayers",    Server.MaxPlayers.ToString() },
						{ "page",          "1" },
						{ "pages",         "1" }
					}),
					Description = Language.GetProcessedMessage("messages.list.ranked.row.empty", new Dictionary<string, string>()),
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
			foreach (string page in pages)
			{
				++pageNum;
				embeds.Add(new EmbedMessage
				{
					Title = Language.GetProcessedMessage("messages.list.ranked.title", new Dictionary<string, string>
					{
						{ "players",       Math.Max(0, Player.Count).ToString() },
						{ "rankedplayers", listItems.Count.ToString()           },
						{ "maxplayers",    Server.MaxPlayers.ToString()         },
						{ "page",          pageNum.ToString()                   },
						{ "pages",         pages.Count.ToString()               }
					}),
					Colour = EmbedMessage.Types.DiscordColour.Cyan,
					Description = page
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
    }
}