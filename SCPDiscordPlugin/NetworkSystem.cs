using Google.Protobuf;
using SCPDiscord.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using LabApi.Features.Wrappers;
using System.Linq;
using System.Threading.Tasks;

namespace SCPDiscord;

public static class NetworkSystem
{
  private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
  public static NetworkStream networkStream { get; private set; } = null;
  private static readonly ConcurrentQueue<MessageWrapper> messageQueue = new();
  private static Stopwatch activityUpdateTimer = new Stopwatch();
  private static int previousActivityPlayerCount = -1;

  private static Thread messageThread;

  private static CancellationTokenSource cts = null;
  private static Task networkTask;

  public static async Task Restart()
  {
    if (networkTask != null && !networkTask.IsCompleted)
      await Stop();

    cts = new CancellationTokenSource();
    networkTask = Task.Run(() => Run(cts.Token), cts.Token);
  }

  public static async Task Stop()
  {
    if (cts == null) return;

    cts.Cancel();

    try
    {
      if (networkTask != null)
      {
        await networkTask;
      }
    }
    catch (OperationCanceledException) { /* ignored */ }
    finally
    {
      cts.Dispose();
      cts = null;
      networkTask = null;
    }
  }

  public static async Task ProcessMessageAsync(List<ulong> channelIDs, string messagePath, Dictionary<string, string> variables)
  {
    string processedMessage = Language.GetProcessedMessage(messagePath, variables);

    // Add time stamp
    if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
    {
      processedMessage = "[" + DateTime.Now.ToString(Config.GetString("settings.timestamp")) + "]: " + processedMessage;
    }

    foreach (ulong channelID in channelIDs)
    {
      MessageWrapper wrapper = new()
      {
        ChatMessage = new ChatMessage
        {
          ChannelID = channelID,
          Content = processedMessage
        }
      };
      QueueMessage(wrapper);
    }
  }

  public static async Task ProcessMessageByIDAsync(ulong channelID, string messagePath, Dictionary<string, string> variables)
  {
    string processedMessage = Language.GetProcessedMessage(messagePath, variables);

    // Add time stamp
    if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
    {
      processedMessage = "[" + DateTime.Now.ToString(Config.GetString("settings.timestamp")) + "]: " + processedMessage;
    }

    MessageWrapper wrapper = new MessageWrapper
    {
      ChatMessage = new ChatMessage
      {
        ChannelID = channelID,
        Content = processedMessage
      }
    };

    QueueMessage(wrapper);
  }

  public static async Task ProcessEmbedMessageAsync(EmbedMessage embed, List<ulong> channelIDs, string messagePath, Dictionary<string, string> variables)
  {
    string processedMessage = Language.GetProcessedMessage(messagePath, variables);
    embed.Description = processedMessage;

    // Add time stamp
    if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
    {
      embed.Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    foreach (ulong channelID in channelIDs)
    {
      // Create copy to avoid pointer issues
      EmbedMessage embedCopy = new EmbedMessage(embed)
      {
        ChannelID = channelID
      };
      MessageWrapper wrapper = new MessageWrapper { EmbedMessage = embedCopy };
      NetworkSystem.QueueMessage(wrapper);
    }
  }

  public static async Task ProcessEmbedMessageByIDAsync(EmbedMessage embed, string messagePath, Dictionary<string, string> variables)
  {
    string processedMessage = Language.GetProcessedMessage(messagePath, variables);
    embed.Description = processedMessage;

    // Add time stamp
    if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
    {
      embed.Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    MessageWrapper wrapper = new MessageWrapper { EmbedMessage = embed };
    NetworkSystem.QueueMessage(wrapper);
  }

  public static async Task Run(CancellationToken ct)
  {
    while ((!Config.ready || !Language.ready) && !ct.IsCancellationRequested)
    {
      try
      {
        await Task.Delay(1000, ct);
      }
      catch (OperationCanceledException) { return; }
    }

    while (!ct.IsCancellationRequested)
    {
      try
      {
        if (IsConnected())
        {
          await Update();
        }
        else
        {
          await Connect();
        }

        await Task.Delay(1000, ct);
      }
      catch (OperationCanceledException) { return; }
      catch (Exception e)
      {
        Logger.Error("Network error caught, if this happens a lot try using the 'scpd_rc' command." + e);
      }
    }

    socket.Disconnect(false);
  }

  private static async Task Update()
  {
    RefreshBotStatus();

    // Send all messages
    for (int i = 0; i < messageQueue.Count; i++)
    {
      if (SendMessage(messageQueue[i]))
      {
        messageQueue.RemoveAt(i);
        i--;
      }
    }

    if (messageQueue.Count != 0)
    {
      Logger.Debug("Could not send all messages.");
    }
  }

  public static bool IsConnected()
  {
    if (socket == null)
    {
      return false;
    }

    try
    {
      return !((socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
    }
    catch (ObjectDisposedException e)
    {
      Logger.Error("TCP client was unexpectedly closed.");
      Logger.Debug(e.ToString());
      return false;
    }
  }

  private static async Task Connect()
  {
    Logger.Debug("Connecting to " + Config.GetString("bot.ip") + ":" + Config.GetInt("bot.port") + "...");

    try
    {
      if (socket != null && socket.IsBound)
      {
        Logger.Warn("Aborting existing message thread...");
        messageThread?.Abort();
        socket.Close();
        await Task.Delay(500);
      }

      socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      socket.Connect(Config.GetString("bot.ip"), Config.GetInt("bot.port"));
      messageThread = new Thread(() => new BotListener());
      messageThread.Start();

      networkStream = new NetworkStream(socket);

      Logger.Info("Connected to Discord bot.");

      EmbedMessage embed = new EmbedMessage
      {
        Colour = EmbedMessage.Types.DiscordColour.Green
      };

      SCPDiscord.SendEmbedWithMessage("messages.connectedtobot", embed);
    }
    catch (SocketException e)
    {
      Logger.Error("Error occurred while connecting to discord bot server: " + e.Message.Trim());
      Logger.Debug(e.ToString());
      await Task.Delay(4000);
    }
    catch (ObjectDisposedException e)
    {
      Logger.Error("TCP client was unexpectedly closed.");
      Logger.Debug(e.ToString());
      await Task.Delay(4000);
    }
    catch (ArgumentOutOfRangeException e)
    {
      Logger.Error("Invalid port.");
      Logger.Debug(e.ToString());
      await Task.Delay(4000);
    }
    catch (ArgumentNullException e)
    {
      Logger.Error("IP address is null.");
      Logger.Debug(e.ToString());
      await Task.Delay(4000);
    }
  }

  private static bool SendMessage(MessageWrapper message)
  {
    if (message == null)
    {
      Logger.Warn("Tried to send message but it was null. " + new StackTrace());
      return true;
    }

    // Abort if client is dead
    if (socket == null || networkStream == null || !socket.Connected)
    {
      Logger.Debug("Error sending message '" + message.MessageCase + "' to bot: Not connected.");
      return false;
    }

    // Try to send the message to the bot
    try
    {
      message.WriteDelimitedTo(networkStream);
      Logger.Debug("Sent packet '" + JsonFormatter.Default.Format(message) + "' to bot.");
      return true;
    }
    catch (Exception e)
    {
      Logger.Error("Error sending packet '" + JsonFormatter.Default.Format(message) + "' to bot.");
      Logger.Error(e.ToString());
      if (!(e is InvalidOperationException || e is ArgumentNullException || e is SocketException))
      {
        throw;
      }
    }

    return false;
  }

  public static async Task QueueMessage(MessageWrapper message)
  {
    if (message == null)
    {
      Logger.Warn("Message was null: \n" + new StackTrace());
      return;
    }

    switch (message.MessageCase)
    {
      case MessageWrapper.MessageOneofCase.EmbedMessage:
        message.EmbedMessage.Description = Language.RunFilters(message.EmbedMessage.ChannelID, message.EmbedMessage.Description);
        break;
      case MessageWrapper.MessageOneofCase.ChatMessage:
        message.ChatMessage.Content = Language.RunFilters(message.ChatMessage.ChannelID, message.ChatMessage.Content);
        break;
      case MessageWrapper.MessageOneofCase.PaginatedMessage:
      case MessageWrapper.MessageOneofCase.BanCommand:
      case MessageWrapper.MessageOneofCase.BotActivity:
      case MessageWrapper.MessageOneofCase.ConsoleCommand:
      case MessageWrapper.MessageOneofCase.KickCommand:
      case MessageWrapper.MessageOneofCase.KickallCommand:
      case MessageWrapper.MessageOneofCase.ListCommand:
      case MessageWrapper.MessageOneofCase.None:
      case MessageWrapper.MessageOneofCase.SyncRoleCommand:
      case MessageWrapper.MessageOneofCase.UnbanCommand:
      case MessageWrapper.MessageOneofCase.UnsyncRoleCommand:
      case MessageWrapper.MessageOneofCase.UserInfo:
      case MessageWrapper.MessageOneofCase.UserQuery:
      case MessageWrapper.MessageOneofCase.MuteCommand:
      case MessageWrapper.MessageOneofCase.ListRankedCommand:
      case MessageWrapper.MessageOneofCase.ListSyncedCommand:
      case MessageWrapper.MessageOneofCase.PlayerInfoCommand:
      default:
        break;
    }

    messageQueue.Add(message);
  }

  private static void RefreshBotStatus()
  {
    if (activityUpdateTimer.Elapsed < TimeSpan.FromSeconds(10) && activityUpdateTimer.IsRunning) return;

    int realPlayers = Player.List.Where(x => x.IsPlayer && x.IsReady).Count();
    // Skip if the player count hasn't changed
    if (previousActivityPlayerCount == realPlayers && activityUpdateTimer.Elapsed < TimeSpan.FromMinutes(1))
    {
      return;
    }

    activityUpdateTimer.Restart();

    // Don't block updates if the server hasn't loaded the max player count yet
    if (Server.MaxPlayers <= 0)
    {
      previousActivityPlayerCount = -1;
    }
    else
    {
      previousActivityPlayerCount = realPlayers;
    }

    // Update player count
    Dictionary<string, string> variables = new Dictionary<string, string>
    {
      { "players",    Math.Max(0, realPlayers).ToString() },
      { "maxplayers", Server.MaxPlayers.ToString()         }
    };

    BotActivity.Types.Status botStatus;
    BotActivity.Types.Activity botActivity;
    string activityText;
    if (realPlayers <= 0)
    {
      botStatus = Utilities.ParseBotStatus(Config.GetString("bot.status.empty"));
      botActivity = Utilities.ParseBotActivity(Config.GetString("bot.activity.empty"));
      activityText = Language.GetProcessedMessage("messages.botactivity.empty", variables);
    }
    else if (realPlayers >= Server.MaxPlayers)
    {
      botStatus = Utilities.ParseBotStatus(Config.GetString("bot.status.full"));
      botActivity = Utilities.ParseBotActivity(Config.GetString("bot.activity.full"));
      activityText = Language.GetProcessedMessage("messages.botactivity.full", variables);
    }
    else
    {
      botStatus = Utilities.ParseBotStatus(Config.GetString("bot.status.active"));
      botActivity = Utilities.ParseBotActivity(Config.GetString("bot.activity.active"));
      activityText = Language.GetProcessedMessage("messages.botactivity.active", variables);
    }

    MessageWrapper wrapper = new MessageWrapper
    {
      BotActivity = new BotActivity
      {
        StatusType = botStatus,
        ActivityType = botActivity,
        ActivityText = activityText
      }
    };

    QueueMessage(wrapper);
  }
}