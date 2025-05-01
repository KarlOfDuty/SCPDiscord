using System.ComponentModel;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord.Commands
{
  public class SyncIDCommand
  {
    [RequireGuild]
    [Command("syncid")]
    [Description("Syncs your Discord role to the server using your SteamID.")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("steam-id")] [Description("Your Steam ID.")] string steamID)
    {
      if (!Utilities.IsPossibleSteamID(steamID, out ulong parsedSteamID))
      {
        DiscordEmbed error = new DiscordEmbedBuilder
        {
          Color = DiscordColor.Red,
          Description = "That SteamID doesn't seem to be valid."
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
          SteamIDOrIP = parsedSteamID.ToString(),
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