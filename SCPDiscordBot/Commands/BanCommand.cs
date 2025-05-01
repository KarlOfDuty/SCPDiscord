using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SCPDiscord.Commands
{
  public class BanCommand
  {
    [RequireGuild]
    [Command("ban")]
    [Description("Bans a player from the server")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("steam-id")] [Description("Steam ID of the player to ban.")] string steamID,
      [Parameter("duration")] [Description("Ban duration (ex: 2d is 2 days).")] string duration,
      [Parameter("reason")] [Description("Reason for the ban.")] string reason)
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
        BanCommand = new Interface.BanCommand
        {
          ChannelID = command.Channel.Id,
          SteamID = parsedSteamID.ToString(),
          Duration = duration,
          Reason = reason,
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