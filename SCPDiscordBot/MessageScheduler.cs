using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord;

public static class MessageScheduler
{
  private static ConcurrentDictionary<ulong, ConcurrentQueue<string>> messageQueues = new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();
  private static List<SlashCommandContext> interactionCache = new List<SlashCommandContext>();
  private static Lock interactionCacheLock = new Lock();

  private static Lock startStopLock = new Lock();
  private static CancellationTokenSource threadCTS;
  private static Task schedulerThread;

  public static void Start()
  {
    using (startStopLock.EnterScope())
    {
      if (schedulerThread != null && !schedulerThread.IsCompleted)
      {
        return;
      }

      threadCTS = new CancellationTokenSource();
      schedulerThread = Init(threadCTS.Token);
    }
  }

  public static async Task Stop()
  {
    using (startStopLock.EnterScope())
    {
      if (threadCTS == null)
      {
        return;
      }
    }

    await threadCTS.CancelAsync();
    try
    {
      await schedulerThread;
    }
    catch (Exception ex)
    {
      Logger.Error("Exception in message scheduler thread: " + ex);
    }

    using (startStopLock.EnterScope())
    {
      schedulerThread = null;
      threadCTS.Dispose();
      threadCTS = null;
    }
  }

  private static async Task Init(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        await Task.Delay(1000, cancellationToken);

        // If we haven't connected to discord yet wait until we do
        if (!DiscordAPI.instance?.connected ?? false)
        {
          continue;
        }

        // Clean old interactions from cache
        using (interactionCacheLock.EnterScope())
        {
          interactionCache.RemoveAll(x => x.Interaction.Id.GetSnowflakeTime() < DateTimeOffset.Now - TimeSpan.FromSeconds(30));
        }

        foreach (KeyValuePair<ulong, ConcurrentQueue<string>> channelQueue in messageQueues)
        {
          StringBuilder finalMessage = new StringBuilder();
          while (channelQueue.Value.TryPeek(out string nextMessage))
          {
            // If message is too long, abort and send the rest next time
            if (finalMessage.Length + nextMessage.Length >= 2000)
            {
              Logger.Warn("Tried to send too much at once (Current: " + finalMessage.Length + " Next: " + nextMessage.Length +
                          "), waiting one second to send the rest.");
              break;
            }

            if (channelQueue.Value.TryDequeue(out nextMessage))
            {
              finalMessage.Append(nextMessage);
              finalMessage.Append('\n');
            }
          }

          string finalMessageStr = finalMessage.ToString();
          if (string.IsNullOrWhiteSpace(finalMessageStr))
          {
            continue;
          }

          if (finalMessageStr.EndsWith('\n'))
          {
            finalMessageStr = finalMessageStr.Remove(finalMessageStr.Length - 1);
          }

          await DiscordAPI.SendMessage(channelQueue.Key, finalMessageStr);
        }
      }
      catch (OperationCanceledException)
      {
        // Expected when task is cancelled
        break;
      }
      catch (Exception e)
      {
        Logger.Error("Message scheduler error: ", e);
      }
    }
  }

  public static void QueueMessage(ulong channelID, string message)
  {
    ConcurrentQueue<string> channelQueue = messageQueues.GetOrAdd(channelID, new ConcurrentQueue<string>());
    channelQueue.Enqueue(message);
  }

  public static bool TryUncacheInteraction(ulong interactionID, out SlashCommandContext interaction)
  {
    using (interactionCacheLock.EnterScope())
    {
      interaction = interactionCache.FirstOrDefault(x => x.Interaction.Id == interactionID);
      return interactionCache.Remove(interaction);
    }
  }

  public static void CacheInteraction(SlashCommandContext interaction)
  {
    using (interactionCacheLock.EnterScope())
    {
      interactionCache.Add(interaction);
    }
  }
}