using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord.Commands
{
  public class UnbanCommand
  {
    [RequireGuild]
    [Command("unban")]
    [Description("Unbans a player from the server")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("steam-id-or-ip")] [Description("Steam ID or IP of the user to unban.")] string steamIDOrIP)
    {
      await command.DeferResponseAsync();
      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        UnbanCommand = new Interface.UnbanCommand
        {
          ChannelID = command.Channel.Id,
          SteamIDOrIP = steamIDOrIP,
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