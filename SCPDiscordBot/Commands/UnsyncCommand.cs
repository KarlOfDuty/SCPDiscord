using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class UnsyncCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("unsync", "Unsyncs your Discord account from the SCP:SL server.")]
		public async Task OnExecute(InteractionContext command)
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				UnsyncRoleCommand = new Interface.UnsyncRoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = command.Member?.Id ?? 0,
					DiscordTag = command.Member?.Username,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending UnsyncCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
