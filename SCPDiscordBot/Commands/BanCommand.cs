using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class BanCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("ban", "Bans a player from the server")]
		public async Task OnExecute(InteractionContext command,
			[Option("SteamID", "Steam ID of the user to ban.")] string steamID,
			[Option("Duration", "Ban duration (ex: 2d is 2 days).")] string duration,
			[Option("Reason", "Reason for the ban.")] string reason)
		{
			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				BanCommand = new Interface.BanCommand
				{
					ChannelID = command.Channel.Id,
					SteamID = steamID,
					Duration = duration,
					AdminTag = command.Member?.Username,
					Reason = reason,
					InteractionID = command.InteractionId
				}
			};

			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending BanCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
