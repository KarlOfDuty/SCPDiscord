using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SCPDiscord.Commands
{
  public class KickCommand
  {
    [RequireGuild]
    [Command("kick")]
    [Description("Kicks a player from the server.")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("steam-id")] [Description("Steam ID of the user to kick.")] string steamID,
      [Parameter("reason")] [Description("Reason for the kick.")] string reason = "")
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
        KickCommand = new Interface.KickCommand
        {
          ChannelID = command.Channel.Id,
          SteamID = parsedSteamID.ToString(),
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