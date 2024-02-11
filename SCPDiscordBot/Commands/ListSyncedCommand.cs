using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class ListSyncedCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("listsynced", "Lists players synced to Discord.")]
		public async Task OnExecute(InteractionContext command, [Option("IncludeOffline", "List all synced players, even offline.")] bool includeOffline = false)
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				ListSyncedCommand = new Interface.ListSyncedCommand
				{
					ChannelID = command.Channel.Id,
					UserID = command.User.Id,
					ListAll = includeOffline,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending ListSyncedCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
