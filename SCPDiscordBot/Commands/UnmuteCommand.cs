using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SCPDiscord.Commands;

public class UnmuteCommand
{
  [RequireGuild]
  [Command("unmute")]
  [Description("Unmutes a player from the server")]
  public async Task OnExecute(SlashCommandContext command,
    [Parameter("steam-id")] [Description("Steam ID of the user to unmute.")] string steamID)
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
      MuteCommand = new Interface.MuteCommand
      {
        ChannelID = command.Channel.Id,
        SteamID = parsedSteamID.ToString(),
        Duration = "0",
        DiscordUserID = command.Member.Id,
        Reason = "",
        InteractionID = command.Interaction.Id,
        DiscordDisplayName = command.Member.DisplayName,
        DiscordUsername = command.Member.Username
      }
    };
    MessageScheduler.CacheInteraction(command);
    await NetworkSystem.SendMessage(message, command);
  }
}