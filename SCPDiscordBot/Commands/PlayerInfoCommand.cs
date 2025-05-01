using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SCPDiscord.Commands
{
  public class PlayerInfoCommand
  {
    [RequireGuild]
    [Command("playerinfo")]
    [Description("Shows general information about a player.")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("steam-id")] [Description("Steam ID of the user to show.")] string steamID)
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
        PlayerInfoCommand = new Interface.PlayerInfoCommand
        {
          ChannelID = command.Channel.Id,
          SteamID = parsedSteamID.ToString(),
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