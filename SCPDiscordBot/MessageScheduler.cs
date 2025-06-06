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

// Separate class to run the thread
public class StartMessageScheduler
{
  public StartMessageScheduler()
  {
    Task _ = MessageScheduler.Init();
  }
}

public static class MessageScheduler
{
  private static ConcurrentDictionary<ulong, ConcurrentQueue<string>> messageQueues = new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();
  private static List<SlashCommandContext> interactionCache = new List<SlashCommandContext>();

  public static async Task Init()
  {
    while (true)
    {
      Thread.Sleep(1000);

      // If we haven't connected to discord yet wait until we do
      if (!DiscordAPI.instance?.connected ?? false)
      {
        continue;
      }

      try
      {
        // Clean old interactions from cache
        interactionCache.RemoveAll(x => x.Interaction.Id.GetSnowflakeTime() < DateTimeOffset.Now - TimeSpan.FromSeconds(30));

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
    interaction = interactionCache.FirstOrDefault(x => x.Interaction.Id == interactionID);
    return interactionCache.Remove(interaction);
  }

  public static void CacheInteraction(SlashCommandContext interaction)
  {
    interactionCache.Add(interaction);
  }
}