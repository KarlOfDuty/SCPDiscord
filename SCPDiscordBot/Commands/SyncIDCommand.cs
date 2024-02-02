using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class SyncSteamIDCommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("syncid", "Syncs your Discord role to the server using your SteamID.")]
		public async Task OnExecute(InteractionContext command, [Option("SteamID", "Your Steam ID.")] string steamID)
		{
			if (steamID.Length < 17)
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "That SteamID doesn't seem to be the right length."
				};
				await command.CreateResponseAsync(error);
				return;
			}

			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				SyncRoleCommand = new Interface.SyncRoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = command.Member?.Id ?? 0,
					DiscordTag = command.Member?.Username,
					SteamIDOrIP = steamID,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending SyncRoleCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
