using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord.Commands
{
  public class ListSyncedCommand
  {
    [RequireGuild]
    [Command("listsynced")]
    [Description("Lists players synced to Discord.")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("include-offline")] [Description("List all synced players, even offline.")] bool includeOffline = false)
    {
      await command.DeferResponseAsync();
      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        ListSyncedCommand = new Interface.ListSyncedCommand
        {
          ChannelID = command.Channel.Id,
          DiscordUserID = command.User.Id,
          ListAll = includeOffline,
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