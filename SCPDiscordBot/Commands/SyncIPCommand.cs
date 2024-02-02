using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class SyncIPCommand : ApplicationCommandModule
	{

		private static readonly string IPV4_PATTERN = "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.){3}(25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)$";

		[SlashRequireGuild]
		[SlashCommand("syncip", "Syncs your Discord role to the server using your IP (non-northwood servers only)")]
		public async Task OnExecute(InteractionContext command, [Option("IP", "Your IP address (IPv4 only).")] string ip)
		{
			if (!Regex.IsMatch(ip, IPV4_PATTERN))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "That IP doesn't seem to be in the right format, it should look something like \"255.255.255.255\"."
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
					SteamIDOrIP = ip,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending SyncRoleCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
