using System.ComponentModel;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord.Commands
{
  public class SyncIPCommand
  {
    private static readonly string IPV4_PATTERN = "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.){3}(25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)$";

    [RequireGuild]
    [Command("syncip")]
    [Description("Syncs your Discord role to the server using your IP (non-northwood servers only)")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("ip")] [Description("Your IP address (IPv4 only).")] string ip)
    {
      if (!Regex.IsMatch(ip, IPV4_PATTERN))
      {
        DiscordEmbed error = new DiscordEmbedBuilder
        {
          Color = DiscordColor.Red,
          Description = "That IP doesn't seem to be in the right format, it should look something like \"255.255.255.255\"."
        };
        await command.RespondAsync(error);
        return;
      }

      await command.DeferResponseAsync();
      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        SyncRoleCommand = new Interface.SyncRoleCommand
        {
          ChannelID = command.Channel.Id,
          DiscordUserID = command.Member?.Id ?? 0,
          SteamIDOrIP = ip,
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