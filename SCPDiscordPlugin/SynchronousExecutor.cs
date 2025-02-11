using System.Collections.Concurrent;
using System.Collections.Generic;
using SCPDiscord.Interface;
using UnityEngine;
using System.Linq;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;

namespace SCPDiscord
{
  public class SynchronousExecutor : MonoBehaviour
  {
    private readonly ConcurrentQueue<ConsoleCommand> queuedCommands = new ConcurrentQueue<ConsoleCommand>();
    private readonly ConcurrentQueue<string> queuedRoleSyncCommands = new ConcurrentQueue<string>();

    public void ScheduleDiscordCommand(ConsoleCommand command)
    {
      queuedCommands.Enqueue(command);
    }

    public void ScheduleRoleSyncCommand(string command)
    {
      queuedRoleSyncCommands.Enqueue(command);
    }

    public void FixedUpdate()
    {
      while (queuedCommands.TryDequeue(out ConsoleCommand command))
      {
        string response = Server.RunCommand(command.Command);

        // Return help command feedback in list form instead
        if (command.Command.StartsWith("help") ||
            command.Command.StartsWith("/help") ||
            command.Command.StartsWith(".help"))
        {
          SendListResponse(command, response);
          continue;
        }

        Dictionary<string, string> variables = new Dictionary<string, string>
        {
          { "feedback", response }
        };

        EmbedMessage embed = new EmbedMessage
        {
          Colour = EmbedMessage.Types.DiscordColour.Orange,
          ChannelID = command.ChannelID,
          InteractionID = command.InteractionID
        };

        SCPDiscord.SendEmbedWithMessageByID(embed, "messages.consolecommandfeedback", variables);
      }

      while (queuedRoleSyncCommands.TryDequeue(out string stringCommand))
      {
        Logger.Debug("RoleSync command response: " + Server.RunCommand(stringCommand));
      }
    }

    private void SendListResponse(ConsoleCommand command, string response)
    {
      List<string> listItems = response.Split('\n').ToList();

      List<EmbedMessage> embeds = new List<EmbedMessage>();

      string title;
      switch (command.Command)
      {
        case string client when client.StartsWith("."):
          title = "Client commands:";
          break;
        case string ra when ra.StartsWith("/"):
          title = "Remote admin commands:";
          break;
        default:
          title = "Server commands:";
          break;
      }

      foreach (string message in Utilities.ParseListIntoMessages(listItems))
      {
        embeds.Add(new EmbedMessage
        {
          Title = title,
          Colour = EmbedMessage.Types.DiscordColour.Cyan,
          Description = message
        });
      }

      PaginatedMessage responsePages = new PaginatedMessage
      {
        ChannelID = command.ChannelID,
        UserID = command.DiscordUserID,
        InteractionID = command.InteractionID
      };
      responsePages.Pages.Add(embeds);

      NetworkSystem.QueueMessage(new MessageWrapper { PaginatedMessage = responsePages });
    }
  }
}