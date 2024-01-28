using System.Collections.Generic;
using SCPDiscord.Interface;

namespace SCPDiscord.BotCommands
{
    public static class KickCommand
    {
        public static void Execute(Interface.KickCommand command)
        {
        	EmbedMessage embed = new EmbedMessage
        	{
        		Colour = EmbedMessage.Types.DiscordColour.Red,
        		ChannelID = command.ChannelID,
        		InteractionID = command.InteractionID
        	};

        	//Perform very basic SteamID validation
        	if (!Utilities.IsPossibleSteamID(command.SteamID))
        	{
        		Dictionary<string, string> variables = new Dictionary<string, string>
        		{
        			{ "userid", command.SteamID }
        		};
        		SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.invalidsteamid", variables);
        		return;
        	}

        	//Get player name for feedback message
        	string playerName = "";
	        Utilities.GetPlayerName(command.SteamID, ref playerName);

        	//Kicks the player
        	if (Utilities.KickPlayer(command.SteamID, command.Reason))
        	{
        		Dictionary<string, string> variables = new Dictionary<string, string>
        		{
        			{ "name", playerName },
        			{ "userid", command.SteamID },
        			{ "admintag", command.AdminTag }
        		};
        		embed.Colour = EmbedMessage.Types.DiscordColour.Green;
		        SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playerkicked", variables);
        	}
        	else
        	{
        		Dictionary<string, string> variables = new Dictionary<string, string>
        		{
        			{ "userid", command.SteamID }
        		};
		        SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playernotfound", variables);
        	}
        }
    }
}