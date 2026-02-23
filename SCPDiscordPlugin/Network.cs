using Google.Protobuf;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using LabApi.Features.Wrappers;
using System.Linq;

namespace SCPDiscord
{
  public class SocketWrapper : IDisposable
  {
    private Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public virtual bool Connected => socket.Connected;
    public virtual bool IsBound => socket.IsBound;
    public virtual int Available => socket.Available;

    public virtual void Connect(string host, int port) => socket.Connect(host, port);
    public virtual void Disconnect() => socket.Disconnect(false);
    public virtual void Close() => socket.Close();

    public virtual bool IsConnected()
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

    public virtual Stream CreateNetworkStream() => new NetworkStream(socket);
    public virtual void Dispose() => socket.Dispose();
  }

  // Separate class to run the thread
  public class StartNetworkSystem
  {
    public StartNetworkSystem()
    {
      Network.Run();
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
        Network.QueueMessage(wrapper);
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

      Network.QueueMessage(wrapper);
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
        Network.QueueMessage(wrapper);
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
      Network.QueueMessage(wrapper);
    }
  }

  public static class Network
  {
    public static SocketWrapper socketWrapper { get; set; } = new SocketWrapper();
    public static Stream networkStream { get; set; } = null;
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
          if (socketWrapper.IsConnected())
          {
            RefreshBotStatus();
            SendQueuedMessages();
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

    private static void SendQueuedMessages()
    {
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

    private static void Connect()
    {
      Logger.Debug("Connecting to " + Config.GetString("bot.ip") + ":" + Config.GetInt("bot.port") + "...");

      try
      {
        if (socketWrapper != null && socketWrapper.IsBound)
        {
          Logger.Warn("Aborting existing message thread...");
          messageThread?.Abort();
          socketWrapper.Close();
          Thread.Sleep(500);
        }

        // If the socketWrapper has been replaced with a mock, don't set it.
        if (socketWrapper == null || !socketWrapper.GetType().IsSubclassOf(typeof(SocketWrapper)))
        {
          socketWrapper = new SocketWrapper();
        }

        socketWrapper.Connect(Config.GetString("bot.ip"), Config.GetInt("bot.port"));
        messageThread = new Thread(() => new BotListener());
        messageThread.Start();

        networkStream = socketWrapper.CreateNetworkStream();

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
      socketWrapper.Disconnect();
    }

    private static bool SendMessage(MessageWrapper message)
    {
      if (message == null)
      {
        Logger.Warn("Tried to send message but it was null. " + new StackTrace());
        return true;
      }

      // Abort if client is dead
      if (socketWrapper == null || networkStream == null || !socketWrapper.Connected)
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
          ActivityText = activityText ?? "SCPDiscord"
        }
      };

      QueueMessage(wrapper);
    }
  }
}