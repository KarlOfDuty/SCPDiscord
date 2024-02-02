using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class UnsyncPlayerCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("unsyncplayer", "Unsyncs a player's Discord account from the SCP:SL server.")]
		public async Task OnExecute(InteractionContext command, [Option("Player", "Player to unsync.")] DiscordUser user)
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				UnsyncRoleCommand = new Interface.UnsyncRoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = user.Id,
					DiscordTag = command.Member?.Username,
					InteractionID = command.InteractionId
				}
			};

			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending UnsyncPlayerCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
