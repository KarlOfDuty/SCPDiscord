﻿using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace SCPDiscord.Commands
{
  public class RACommand
  {
    [RequireGuild]
    [Command("ra")]
    [Description("Runs a remote admin command.")]
    public async Task OnExecute(SlashCommandContext command,
      [Parameter("Command")] [Description("Remote admin command to run.")] string serverCommand)
    {
      if (!ConfigParser.HasPermission(command.Member, serverCommand))
      {
        DiscordEmbed error = new DiscordEmbedBuilder
        {
          Color = DiscordColor.Red,
          Description = "You do not have permission to use that command."
        };
        await command.RespondAsync(error);
        return;
      }

      await command.DeferResponseAsync();
      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        ConsoleCommand = new Interface.ConsoleCommand
        {
          ChannelID = command.Channel.Id,
          DiscordUserID = command.Member?.Id ?? 0,
          Command = "/" + serverCommand,
          InteractionID = command.Interaction.Id,
          DiscordDisplayName = command.Member?.DisplayName,
          DiscordUsername = command.Member?.Username
        }
      };
      MessageScheduler.CacheInteraction(command);
      await NetworkSystem.SendMessage(message, command);
    }
  }
}