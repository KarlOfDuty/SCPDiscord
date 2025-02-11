using Google.Protobuf;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using LabApi.Features.Wrappers;

namespace SCPDiscord
{
  // Separate class to run the thread
  public class StartNetworkSystem
  {
    public StartNetworkSystem()
    {
      NetworkSystem.Run();
    }
  }

  public class ProcessMessageAsync
  {
    public ProcessMessageAsync(List<ulong> channelIDs, string messagePath, Dictionary<string, string> variables)
    {
      string processedMessage = Language.GetProcessedMessage(messagePath, variables);

      // Add time stamp
      if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
      {
        processedMessage = "[" + DateTime.Now.ToString(Config.GetString("settings.timestamp")) + "]: " + processedMessage;
      }

      foreach (ulong channelID in channelIDs)
      {
        MessageWrapper wrapper = new MessageWrapper
        {
          ChatMessage = new ChatMessage
          {
            ChannelID = channelID,
            Content = processedMessage
          }
        };
        NetworkSystem.QueueMessage(wrapper);
      }
    }
  }

  public class ProcessMessageByIDAsync
  {
    public ProcessMessageByIDAsync(ulong channelID, string messagePath, Dictionary<string, string> variables)
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

      NetworkSystem.QueueMessage(wrapper);
    }
  }

  public class ProcessEmbedMessageAsync
  {
    public ProcessEmbedMessageAsync(EmbedMessage embed, List<ulong> channelIDs, string messagePath, Dictionary<string, string> variables)
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
  }

  public class ProcessEmbedMessageByIDAsync
  {
    public ProcessEmbedMessageByIDAsync(EmbedMessage embed, string messagePath, Dictionary<string, string> variables)
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
  }

  public static class NetworkSystem
  {
    private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static NetworkStream networkStream { get; private set; } = null;
    private static readonly List<MessageWrapper> messageQueue = new List<MessageWrapper>();
    private static Stopwatch activityUpdateTimer = new Stopwatch();
    private static int previousActivityPlayerCount = -1;

    private static Thread messageThread;

    public static void Run()
    {
      while (!Config.ready || !Language.ready)
      {
        Thread.Sleep(1000);
      }

      while (!SCPDiscord.plugin.shutdown)
      {
        try
        {
          if (IsConnected())
          {
            Update();
          }
          else
          {
            Connect();
          }

          Thread.Sleep(1000);
        }
        catch (Exception e)
        {
          Logger.Error("Network error caught, if this happens a lot try using the 'scpd_rc' command." + e);
        }
      }
    }

    private static void Update()
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

    private static void Connect()
    {
      Logger.Debug("Connecting to " + Config.GetString("bot.ip") + ":" + Config.GetInt("bot.port") + "...");

      try
      {
        if (socket != null && socket.IsBound)
        {
          Logger.Warn("Aborting existing message thread...");
          messageThread?.Abort();
          socket.Close();
          Thread.Sleep(500);
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
        Logger.Error("Error occured while connecting to discord bot server: " + e.Message.Trim());
        Logger.Debug(e.ToString());
        Thread.Sleep(4000);
      }
      catch (ObjectDisposedException e)
      {
        Logger.Error("TCP client was unexpectedly closed.");
        Logger.Debug(e.ToString());
        Thread.Sleep(4000);
      }
      catch (ArgumentOutOfRangeException e)
      {
        Logger.Error("Invalid port.");
        Logger.Debug(e.ToString());
        Thread.Sleep(4000);
      }
      catch (ArgumentNullException e)
      {
        Logger.Error("IP address is null.");
        Logger.Debug(e.ToString());
        Thread.Sleep(4000);
      }
    }

    public static void Disconnect()
    {
      socket.Disconnect(false);
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

    public static void QueueMessage(MessageWrapper message)
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

      // Skip if the player count hasn't changed
      if (previousActivityPlayerCount == Player.Count && activityUpdateTimer.Elapsed < TimeSpan.FromMinutes(5))
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
        previousActivityPlayerCount = Player.Count;
      }

      // Update player count
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "players",    Math.Max(0, Player.Count).ToString() },
        { "maxplayers", Server.MaxPlayers.ToString()         }
      };

      BotActivity.Types.Status botStatus;
      BotActivity.Types.Activity botActivity;
      string activityText;
      if (Player.Count <= 0)
      {
        botStatus = Utilities.ParseBotStatus(Config.GetString("bot.status.empty"));
        botActivity = Utilities.ParseBotActivity(Config.GetString("bot.activity.empty"));
        activityText = Language.GetProcessedMessage("messages.botactivity.empty", variables);
      }
      else if (Player.Count >= Server.MaxPlayers)
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
}