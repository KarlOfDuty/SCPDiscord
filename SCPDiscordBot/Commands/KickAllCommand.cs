using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord.Commands
{
  public class KickAllCommand
  {
    [RequireGuild]
    [Command("kickall")]
    [Description("Kicks all players on the server.")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("reason")] [Description("Kick reason.")] string kickReason = "")
    {
      await command.DeferResponseAsync();
      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        KickallCommand = new Interface.KickallCommand
        {
          ChannelID = command.Channel.Id,
          Reason = kickReason,
          InteractionID = command.Interaction.Id,
          DiscordDisplayName = command.Member.DisplayName,
          DiscordUsername = command.Member.Username,
          DiscordUserID = command.Member.Id
        }
      };
      MessageScheduler.CacheInteraction(command);
      await NetworkSystem.SendMessage(message, command);
    }
  }
}