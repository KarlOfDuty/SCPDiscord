using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SCPDiscord.Commands
{
  public class UnsyncPlayerCommand
  {
    [RequireGuild]
    [Command("unsyncplayer")]
    [Description("Unsyncs a player's Discord account from the SCP:SL server.")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("player")] [Description("Player to unsync.")] DiscordUser user)
    {
      await command.DeferResponseAsync();
      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        UnsyncRoleCommand = new Interface.UnsyncRoleCommand
        {
          ChannelID = command.Channel.Id,
          DiscordUserID = user.Id,
          InteractionID = command.Interaction.Id,
          DiscordDisplayName = command.Member.DisplayName,
          DiscordUsername = command.Member.Username
        }
      };

      MessageScheduler.CacheInteraction(command);
      await NetworkSystem.SendMessage(message, command);
    }
  }
}