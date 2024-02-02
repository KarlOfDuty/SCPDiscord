using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class KickCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("kick", "Kicks a player from the server.")]
		public async Task OnExecute(InteractionContext command, [Option("SteamID", "Steam ID of the user to kick.")] string steamID, [Option("Reason", "Reason for the kick.")] string reason = "")
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				KickCommand = new Interface.KickCommand
				{
					ChannelID = command.Channel.Id,
					SteamID = steamID,
					AdminTag = command.Member?.Username,
					Reason = reason,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending KickCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
