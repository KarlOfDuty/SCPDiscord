using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using SCPDiscord.Commands;

namespace SCPDiscord;

public class DiscordAPI
{
  public static DiscordAPI instance { get; private set; }
  public bool connected { get; internal set; }

  private static DiscordClient client = null;

  public static async Task Init()
  {
    try
    {
      Logger.Log("Setting up Discord client...");

      instance = new DiscordAPI();

      DiscordClientBuilder clientBuilder = DiscordClientBuilder.CreateDefault(ConfigParser.Config.bot.token, DiscordIntents.All).SetReconnectOnFatalGatewayErrors();

        clientBuilder.ConfigureServices(configure =>
        {
            configure.AddSingleton<IClientErrorHandler>(new ErrorHandler());
        });

        clientBuilder.ConfigureEventHandlers(builder =>
        {
            builder.HandleGuildDownloadCompleted(EventHandler.OnReady);
            builder.HandleGuildAvailable(EventHandler.OnGuildAvailable);
        });

        clientBuilder.UseInteractivity(new InteractivityConfiguration
        {
            PaginationBehaviour = PaginationBehaviour.Ignore,
            PaginationDeletion = PaginationDeletion.DeleteMessage,
            Timeout = TimeSpan.FromMinutes(15)
        });

        if (!ConfigParser.Config.bot.disableCommands)
        {
          clientBuilder.UseCommands((_, extension) =>
          {
              extension.AddCommands(
              [
                typeof(BanCommand),
                typeof(HelpCommand),
                typeof(KickAllCommand),
                typeof(KickCommand),
                typeof(ListCommand),
                typeof(ListRankedCommand),
                typeof(ListSyncedCommand),
                typeof(MuteCommand),
                typeof(PlayerInfoCommand),
                typeof(RACommand),
                typeof(ServerCommand),
                typeof(SyncIDCommand),
                typeof(SyncIPCommand),
                typeof(UnbanCommand),
                typeof(UnmuteCommand),
                typeof(UnsyncCommand),
                typeof(UnsyncPlayerCommand)
              ]);
              extension.AddProcessor(new SlashCommandProcessor());
              extension.CommandErrored += EventHandler.OnCommandError;
          }, new CommandsConfiguration
          {
              RegisterDefaultCommandProcessors = false,
              UseDefaultCommandErrorHandler = false
          });
        }

        clientBuilder.ConfigureExtraFeatures(clientConfig =>
        {
            clientConfig.LogUnknownEvents = false;
            clientConfig.LogUnknownAuditlogs = false;
        });

        clientBuilder.ConfigureLogging(config =>
        {
            config.AddProvider(new LoggerProvider());
        });

        client = clientBuilder.Build();

        Logger.Log("Connecting to Discord...");
        await client.ConnectAsync();

        if (ConfigParser.Config.bot.disableCommands)
        {
          await client.BulkOverwriteGlobalApplicationCommandsAsync([]);
        }
    }
    catch (Exception e)
    {
      Logger.Fatal("Failed to initialize Discord client.", e);
    }
  }

  public static void SetDisconnectedActivity()
  {
    if (!Enum.TryParse(ConfigParser.Config.bot.presenceType, true, out DiscordActivityType activityType))
    {
      Logger.Warn("Activity type '" + ConfigParser.Config.bot.presenceType + "' invalid, using 'Playing' instead.");
      activityType = DiscordActivityType.Playing;
    }

    if (!Enum.TryParse(ConfigParser.Config.bot.statusType, true, out DiscordUserStatus statusType))
    {
      Logger.Warn("Status type '" + ConfigParser.Config.bot.statusType + "' invalid, using 'DoNotDisturb' instead.");
      statusType = DiscordUserStatus.DoNotDisturb;
    }

    SetActivity(ConfigParser.Config.bot.presenceText, activityType, statusType);
  }

  public static void SetActivity(string activityText, DiscordActivityType activityType, DiscordUserStatus status)
  {
    if (instance.connected)
    {
      client.UpdateStatusAsync(new DiscordActivity(activityText, activityType), status);
    }
  }

  public static async Task SendMessage(ulong channelID, string message)
  {
    if (!instance.connected)
    {
      return;
    }

    try
    {
      DiscordChannel channel = await client.GetChannelAsync(channelID);
      try
      {
        await channel.SendMessageAsync(message);
      }
      catch (UnauthorizedException)
      {
        Logger.Error("No permissions to send message in '" + channel.Name + "'");
      }
    }
    catch (Exception)
    {
      Logger.Error("Could not send message in text channel '" + channelID + "'");
    }
  }

  public static async Task SendMessage(ulong channelID, DiscordEmbed message)
  {
    if (!instance.connected)
    {
      return;
    }

    try
    {
      DiscordChannel channel = await client.GetChannelAsync(channelID);
      try
      {
        await channel.SendMessageAsync(message);
      }
      catch (UnauthorizedException)
      {
        Logger.Error("No permissions to send message in '" + channel.Name + "'");
      }
    }
    catch (Exception)
    {
      Logger.Error("Could not send embed in text channel '" + channelID + "'");
    }
  }

  public static async Task SendInteractionResponse(ulong interactionID, ulong channelID, DiscordEmbed message)
  {
    if (!instance.connected)
    {
      return;
    }

    try
    {
      try
      {
        if (MessageScheduler.TryUncacheInteraction(interactionID, out SlashCommandContext interaction))
        {
          await interaction.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(message));
        }
        else
        {
          Logger.Error("Couldn't find interaction in the cache, sending as normal message instead.");
          await SendMessage(channelID, message);
        }
      }
      catch (UnauthorizedException)
      {
        Logger.Error("No permissions to send command response.");
      }
    }
    catch (Exception e)
    {
      Logger.Error("Could not send command response.", e);
    }
  }

  public static async Task SendPaginatedResponse(ulong interactionID, ulong channelID, ulong userID, List<Page> message)
  {
    if (!instance.connected)
    {
      return;
    }

    try
    {
      try
      {
        if (MessageScheduler.TryUncacheInteraction(interactionID, out SlashCommandContext interaction))
        {
          _ = interaction.Interaction.SendPaginatedResponseAsync(false, interaction.User, message);
        }
        else
        {
          Logger.Error("Couldn't find interaction in the cache, sending as normal message instead.");
          await SendPaginatedMessage(channelID, userID, message);
        }
      }
      catch (UnauthorizedException)
      {
        Logger.Error("No permissions to send command response.");
      }
    }
    catch (Exception e)
    {
      Logger.Error("Could not send command response.", e);
    }
  }

  public static async Task SendPaginatedMessage(ulong channelID, ulong userID, IEnumerable<Page> pages)
  {
    if (!instance.connected)
    {
      return;
    }

    try
    {
      try
      {
        DiscordChannel channel = await client.GetChannelAsync(channelID);
        DiscordUser user = await client.GetUserAsync(userID);

        _ = channel.SendPaginatedMessageAsync(user, pages);
      }
      catch (UnauthorizedException)
      {
        Logger.Error("No permissions to send command response.");
      }
    }
    catch (Exception e)
    {
      Logger.Error("Could not send command response.", e);
    }
  }

  public static async Task GetPlayerRoles(ulong userID, string steamID)
  {
    if (!instance.connected)
    {
      return;
    }

    if (ConfigParser.Config.bot.serverId == 0)
    {
      Logger.Warn("Plugin attempted to use role sync, but no server ID was set in the config. Ignoring request...");
      return;
    }

    try
    {
      DiscordGuild guild = await client.GetGuildAsync(ConfigParser.Config.bot.serverId);
      DiscordMember member = await guild.GetMemberAsync(userID);

      Interface.MessageWrapper message = new Interface.MessageWrapper
      {
        UserInfo = new Interface.UserInfo
        {
          DiscordUserID = userID,
          SteamIDOrIP = steamID,
          DiscordDisplayName = member.DisplayName,
          DiscordUsername = member.Username
        }
      };
      message.UserInfo.RoleIDs.AddRange(member.Roles.Select(role => role.Id));
      message.UserInfo.RoleIDs.Add(member.Guild.EveryoneRole.Id);
      await NetworkSystem.SendMessage(message, null);
    }
    catch (Exception e)
    {
      Logger.Warn("Couldn't find discord server or server member for role syncing requested by plugin. Discord ID: "
                  + userID + " SteamID/IP: " + steamID, e);
    }
  }
}