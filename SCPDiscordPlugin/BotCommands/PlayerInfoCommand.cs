using System;
using System.Collections.Generic;
using PluginAPI.Core;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
    public class PlayerInfoCommand
    {
	    public static void Execute(Interface.PlayerInfoCommand command)
		{
			// TODO: Populate with player info

			/*PaginatedMessage response = new PaginatedMessage
			{
				ChannelID = command.ChannelID,
				UserID = command.UserID,
				InteractionID = command.InteractionID
			};
			response.Pages.Add(embeds);

			NetworkSystem.QueueMessage(new MessageWrapper { PaginatedMessage = response });*/
		}
    }
}