using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace SCPDiscord.Commands
{
	public class RACommand : ApplicationCommandModule
	{
		[SlashRequireGuild]
		[SlashCommand("ra", "Runs a remote admin command.")]
		public async Task OnExecute(InteractionContext command, [Option("Command", "Remote admin command to run.")] string serverCommand)
		{
			if (!ConfigParser.HasPermission(command.Member, serverCommand))
			{
				DiscordEmbed error = new DiscordEmbedBuilder
				{
					Color = DiscordColor.Red,
					Description = "You do not have permission to use that command."
				};
				await command.CreateResponseAsync(error);
				return;
			}

			await command.DeferAsync();
			Interface.MessageWrapper message = new Interface.MessageWrapper
			{
				ConsoleCommand = new Interface.ConsoleCommand
				{
					ChannelID = command.Channel.Id,
					DiscordID = command.Member?.Id ?? 0,
					Command = "/" + serverCommand,
					InteractionID = command.InteractionId
				}
			};
			MessageScheduler.CacheInteraction(command);
			await NetworkSystem.SendMessage(message, command);
			Logger.Debug("Sending ConsoleCommand to plugin from @" + command.Member?.Username, LogID.DISCORD);
		}
	}
}
