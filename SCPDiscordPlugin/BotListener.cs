using SCPDiscord.BotCommands;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SCPDiscord
{
  public static class BotListener
  {
    private static CancellationTokenSource cts = null;
    private static Task listenerTask;
    private static readonly SemaphoreSlim restartGate = new(1, 1);

    public static async Task Restart()
    {
      await restartGate.WaitAsync().ConfigureAwait(false);
      try
      {
        if (listenerTask != null && !listenerTask.IsCompleted)
        {
          await Stop().ConfigureAwait(false);
        }

        cts = new CancellationTokenSource();
        listenerTask = Task.Run(() => Listen(cts.Token), cts.Token);
      }
      finally
      {
        restartGate.Release();
      }
    }

    public static async Task Stop()
    {
      if (cts == null) return;

      cts.Cancel();
      try
      {
        if (listenerTask != null)
        {
          await listenerTask.ConfigureAwait(false);
        }
      }
      catch (OperationCanceledException) { /* ignored */ }
      finally
      {
        cts.Dispose();
        cts = null;
        listenerTask = null;
      }
    }

    // This function is pretty structurally complicated in order to both do error handling for different
    // scenarios and making it possible to cancel it even when the tread is blocked by reading from the socket.
    private static async Task Listen(CancellationToken ct)
    {
      CancellationTokenRegistration reg = default;
      try
      {
        // Close the network stream if the task is cancelled, but we are blocked by reading from it.
        reg = ct.Register(() => {
          try { NetworkSystem.networkStream?.Close(); } catch { }
        });

        while (!ct.IsCancellationRequested)
        {
          try
          {
            if (NetworkSystem.IsConnected())
            {
              Interface.MessageWrapper data;
              try
              {
                data = Interface.MessageWrapper.Parser.ParseDelimitedFrom(NetworkSystem.networkStream);
              }
              catch (Exception ex) when (ex is IOException or ObjectDisposedException)
              {
                // Do nothing if this task is cancelling
                if (!ct.IsCancellationRequested)
                {
                  Logger.Error("Connection to bot lost.");
                  _ = NetworkSystem.Restart();
                }
                return;
              }

              HandleIncomingMessage(data);
            }
            await Task.Delay(500, ct).ConfigureAwait(false);
          }
          catch (OperationCanceledException)
          {
            return;
          }
          catch (Exception ex)
          {
            Logger.Error("BotListener Error: " + ex);
          }
        }
      }
      finally
      {
        reg.Dispose();
      }
    }

    private static void HandleIncomingMessage(Interface.MessageWrapper data)
    {
      Logger.Debug("Incoming packet: " + Google.Protobuf.JsonFormatter.Default.Format(data));

      switch (data.MessageCase)
      {
        case Interface.MessageWrapper.MessageOneofCase.SyncRoleCommand:
          SCPDiscord.SendEmbedByID(RoleSync.AddPlayer(data.SyncRoleCommand));
          break;

        case Interface.MessageWrapper.MessageOneofCase.UnsyncRoleCommand:
          SCPDiscord.SendEmbedByID(RoleSync.RemovePlayer(data.UnsyncRoleCommand));
          break;

        case Interface.MessageWrapper.MessageOneofCase.ConsoleCommand:
          SCPDiscord.plugin.sync.ScheduleDiscordCommand(data.ConsoleCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.UserInfo:
          RoleSync.ReceiveQueryResponse(data.UserInfo);
          break;

        case Interface.MessageWrapper.MessageOneofCase.BanCommand:
          BanCommand.Execute(data.BanCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.UnbanCommand:
          UnbanCommand.Execute(data.UnbanCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.KickCommand:
          KickCommand.Execute(data.KickCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.KickallCommand:
          KickallCommand.Execute(data.KickallCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.ListCommand:
          ListCommand.Execute(data.ListCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.ListRankedCommand:
          ListRankedCommand.Execute(data.ListRankedCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.ListSyncedCommand:
          ListSyncedCommand.Execute(data.ListSyncedCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.MuteCommand:
          MuteCommand.Execute(data.MuteCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.PlayerInfoCommand:
          PlayerInfoCommand.Execute(data.PlayerInfoCommand);
          break;

        case Interface.MessageWrapper.MessageOneofCase.BotActivity:
        case Interface.MessageWrapper.MessageOneofCase.ChatMessage:
        case Interface.MessageWrapper.MessageOneofCase.UserQuery:
        case Interface.MessageWrapper.MessageOneofCase.PaginatedMessage:
        case Interface.MessageWrapper.MessageOneofCase.EmbedMessage:
          Logger.Error("Received packet meant for bot: " + Google.Protobuf.JsonFormatter.Default.Format(data));
          break;

        case Interface.MessageWrapper.MessageOneofCase.None:
        default:
          Logger.Warn("Unknown packet received: " + Google.Protobuf.JsonFormatter.Default.Format(data));
          break;
      }
    }
  }
}