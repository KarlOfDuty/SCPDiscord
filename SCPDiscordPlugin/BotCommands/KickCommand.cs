using System.Collections.Generic;
using PluginAPI.Core;
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
        	if (!Utilities.IsPossibleSteamID(command.SteamID, out ulong _))
        	{
        		Dictionary<string, string> vars = new Dictionary<string, string>
        		{
        			{ "userid", command.SteamID }
        		};
        		SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.invalidsteamid", vars);
        		return;
        	}

        	//Get player name for feedback message
	        if (!Player.TryGet(command.SteamID, out Player player))
	        {
		        Dictionary<string, string> vars = new Dictionary<string, string>
		        {
			        { "userid", command.SteamID }
		        };
		        SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playernotfound", vars);
	        }

	        if (string.IsNullOrWhiteSpace(command.Reason))
	        {
		        command.Reason = "Kicked by server moderators";
	        }

        	//Kicks the player
        	Dictionary<string, string> variables = new Dictionary<string, string>
        	{
		        { "reason", command.Reason },
        		{ "admintag", command.AdminTag }
        	};
	        variables.AddPlayerVariables(player, "player");
        	embed.Colour = EmbedMessage.Types.DiscordColour.Green;

	        player.Kick(command.Reason);
	        SCPDiscord.plugin.SendEmbedWithMessageByID(embed, "messages.playerkicked", variables);

        }
    }
}