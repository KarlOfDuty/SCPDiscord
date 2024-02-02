using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class KickAllCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("kickall", "Kicks all players on the server.")]
		public async Task OnExecute(InteractionContext command, [Option("Reason", "Kick reason.")] string kickReason = "")
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				KickallCommand = new Interface.KickallCommand
				{
					ChannelID = command.Channel.Id,
					AdminTag = command.Member?.Username,
					Reason = kickReason,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending KickallCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
