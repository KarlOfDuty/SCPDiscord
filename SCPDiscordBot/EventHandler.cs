using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

namespace SCPDiscord;

public static class EventHandler
{
  internal static bool hasLoggedGuilds = false;

  public static Task OnReady(DiscordClient discordClient, GuildDownloadCompletedEventArgs e)
  {
    DiscordAPI.instance.connected = true;
    hasLoggedGuilds = true;
    Logger.Log("Connected to Discord.");
    DiscordAPI.SetDisconnectedActivity();
    return Task.CompletedTask;
  }

  public static async Task OnGuildAvailable(DiscordClient discordClient, GuildCreatedEventArgs e)
  {
    if (hasLoggedGuilds)
    {
      return;
    }

    Logger.Log("Found Discord server: " + e.Guild.Name + " (" + e.Guild.Id + ")");

    if (SCPDiscordBot.commandLineArgs.serversToLeave.Contains(e.Guild.Id))
    {
      Logger.Warn("LEAVING DISCORD SERVER AS REQUESTED: " + e.Guild.Name + " (" + e.Guild.Id + ")");
      await e.Guild.LeaveAsync();
      return;
    }

    IReadOnlyDictionary<ulong, DiscordRole> roles = e.Guild.Roles;

    foreach ((ulong roleID, DiscordRole role) in roles)
    {
      Logger.Debug(role.Name.PadRight(40, '.') + roleID);
    }
  }

  public static async Task OnCommandError(CommandsExtension commandSystem, CommandErroredEventArgs e)
  {
    switch (e.Exception)
    {
      case ArgumentException:
      {
        await e.Context.Channel.SendMessageAsync(new DiscordEmbedBuilder
        {
          Color = DiscordColor.Red,
          Description = "Internal error occurred."
        });
        return;
      }

      case ChecksFailedException ex:
      {
        foreach (ContextCheckFailedData error in ex.Errors)
        {
          await e.Context.Channel.SendMessageAsync(new DiscordEmbedBuilder
          {
            Color = DiscordColor.Red,
            Description = error.ErrorMessage
          });
        }

        return;
      }

      case BadRequestException ex:
      {
        Logger.Error("Command exception occurred:\n" + e.Exception);
        Logger.Error("JSON Message: " + ex.JsonMessage);
        return;
      }

      default:
      {
        Logger.Error("Exception occurred.", e.Exception);
        await e.Context.Channel.SendMessageAsync(new DiscordEmbedBuilder
        {
          Color = DiscordColor.Red,
          Description = "Internal error occurred, please report this to the developer."
        });
        return;
      }
    }
  }
}

internal class ErrorHandler : IClientErrorHandler
{
  public ValueTask HandleEventHandlerError(string name, Exception exception, Delegate invokedDelegate, object sender, object args)
  {
    Logger.Error("Client exception occurred:\n" + exception);
    if (exception is BadRequestException ex)
    {
      Logger.Error("JSON Message: " + ex.JsonMessage);
    }

    return ValueTask.FromException(exception);
  }

  public ValueTask HandleGatewayError(Exception exception)
  {
    Logger.Error("A gateway error occurred:\n" + exception);
    return ValueTask.FromException(exception);
  }
}