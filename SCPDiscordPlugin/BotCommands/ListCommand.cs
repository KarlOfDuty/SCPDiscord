using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
  public static class ListCommand
  {
    public static void Execute(Interface.ListCommand command)
    {
      // Create empty list if there are no players online
      if (Player.Count == 0)
      {
        EmbedMessage embed = new EmbedMessage
        {
          Title = Language.GetProcessedMessage("messages.list.default.title", new Dictionary<string, string>
          {
            { "players",             Math.Max(0, Player.Count).ToString() },
            { "maxplayers",          Server.MaxPlayers.ToString() },
            { "page",                "1" },
            { "pages",               "1" },
            { "discord-displayname", command.DiscordDisplayName },
            { "discord-username",    command.DiscordUsername },
            { "discord-userid",      command.DiscordUserID.ToString() },
          }),
          Description = Language.GetProcessedMessage("messages.list.default.row.empty", new Dictionary<string, string>()),
          Colour = EmbedMessage.Types.DiscordColour.Red,
          ChannelID = command.ChannelID,
          InteractionID = command.InteractionID
        };
        SCPDiscord.SendEmbedByID(embed);
        return;
      }

      // Create list of player rows
      List<string> listItems = new List<string>();
      foreach (Player player in Player.List)
      {
        Dictionary<string, string> variables = new Dictionary<string, string> { };
        variables.AddPlayerVariables(player, "player");
        string row = Language.GetProcessedMessage("messages.list.default.row.default", variables);

        listItems.Add(Language.RunFilters(command.ChannelID, player, row));
      }

      // Separate list into pages
      List<EmbedMessage> embeds = new List<EmbedMessage>();
      int pageNum = 0;
      LinkedList<string> pages = Utilities.ParseListIntoMessages(listItems);
      foreach (string page in pages)
      {
        ++pageNum;
        embeds.Add(new EmbedMessage
        {
          Title = Language.GetProcessedMessage("messages.list.default.title", new Dictionary<string, string>
          {
            { "players",             Math.Max(0, Player.Count).ToString() },
            { "maxplayers",          Server.MaxPlayers.ToString() },
            { "page",                pageNum.ToString() },
            { "pages",               pages.Count.ToString() },
            { "discord-displayname", command.DiscordDisplayName },
            { "discord-username",    command.DiscordUsername },
            { "discord-userid",      command.DiscordUserID.ToString() },
          }),
          Colour = EmbedMessage.Types.DiscordColour.Cyan,
          Description = page
        });
      }

      PaginatedMessage response = new PaginatedMessage
      {
        ChannelID = command.ChannelID,
        UserID = command.DiscordUserID,
        InteractionID = command.InteractionID
      };
      response.Pages.Add(embeds);

      NetworkSystem.QueueMessage(new MessageWrapper { PaginatedMessage = response });
    }
  }
}