using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class ListRankedCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("listranked", "Lists online players with server ranks.")]
		public async Task OnExecute(InteractionContext command)
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				ListRankedCommand = new Interface.ListRankedCommand
				{
					ChannelID = command.Channel.Id,
					UserID = command.User.Id,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending ListRankedCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
