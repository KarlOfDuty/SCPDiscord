using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class UnbanCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("unban", "Unbans a player from the server")]
		public async Task OnExecute(InteractionContext command, [Option("SteamIDorIP", "Steam ID or IP of the user to unban.")] string steamIDOrIP)
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				UnbanCommand = new Interface.UnbanCommand
				{
					ChannelID = command.Channel.Id,
					SteamIDOrIP = steamIDOrIP,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending UnbanCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
