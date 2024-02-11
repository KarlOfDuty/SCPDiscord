using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class PlayerInfoCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("playerinfo", "Shows general information about a player.")]
		public async Task OnExecute(InteractionContext command, [Option("SteamID", "Steam ID of the user to show.")] string steamID)
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				PlayerInfoCommand = new Interface.PlayerInfoCommand
				{
					ChannelID = command.Channel.Id,
					SteamID = steamID,
					InteractionID = command.InteractionId
				}
			};

			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending PlayerInfoCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
