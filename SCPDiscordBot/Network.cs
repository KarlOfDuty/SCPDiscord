using DSharpPlus.Entities;
using Google.Protobuf;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace SCPDiscord
{
  // Separate class to run the thread
  public class StartNetworkSystem
  {
    public StartNetworkSystem()
    {
      NetworkSystem.Init();
    }
  }

  public static class NetworkSystem
  {
    private static Socket clientSocket = null;
    private static Socket listenerSocket = null;
    private static NetworkStream networkStream = null;

    public static void Init()
    {
      if (listenerSocket != null)
      {
        listenerSocket.Shutdown(SocketShutdown.Both);
        listenerSocket.Close();
      }

      if (clientSocket != null)
      {
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
      }

      while (!ConfigParser.loaded)
      {
        Thread.Sleep(1000);
      }

      IPAddress ipAddress;

      if (ConfigParser.config.plugin.address == "0.0.0.0")
      {
        ipAddress = IPAddress.Any;
      }
      else if (ConfigParser.config.plugin.address == "::0")
      {
        ipAddress = IPAddress.IPv6Any;
      }
      else if (IPAddress.TryParse(ConfigParser.config.plugin.address, out IPAddress parsedIP))
      {
        ipAddress = parsedIP;
      }
      else
      {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(ConfigParser.config.plugin.address);

        // Use an IPv4 address if available
        if (ipHostInfo.AddressList.Any(ip => ip.AddressFamily == AddressFamily.InterNetwork))
        {
          ipAddress = ipHostInfo.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
        else
        {
          ipAddress = ipHostInfo.AddressList[0];
        }
      }

      IPEndPoint listenerEndpoint = new IPEndPoint(ipAddress, ConfigParser.config.plugin.port);
      listenerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      listenerSocket.Bind(listenerEndpoint);
      listenerSocket.Listen(10);

      while (true)
      {
        try
        {
          if (IsConnected())
          {
            Task _ = Update();
          }
          else
          {
            DiscordAPI.SetDisconnectedActivity();
            Logger.Log("Listening on " + ipAddress + ":" + ConfigParser.config.plugin.port);
            clientSocket = listenerSocket.Accept();
            networkStream = new NetworkStream(clientSocket, true);
            Logger.Log("Plugin connected.");
          }
        }
        catch (Exception e)
        {
          Logger.Error("Network error caught, if this happens a lot try using the 'scpd_rc' command." , e);
        }
      }
    }

    private static async Task Update()
    {
      MessageWrapper wrapper;
      try
      {
        wrapper = MessageWrapper.Parser.ParseDelimitedFrom(networkStream);
      }
      catch (InvalidProtocolBufferException e)
      {
        Logger.Error("Incoming packet was not in the required format!", e, true);
        return;
      }
      catch (Exception e)
      {
        Logger.Error("Couldn't parse incoming packet!", e, true);
        return;
      }

      Logger.Debug("Incoming packet: " + JsonFormatter.Default.Format(wrapper));

      switch (wrapper.MessageCase)
      {
        case MessageWrapper.MessageOneofCase.BotActivity:
          try
          {
            DiscordAPI.SetActivity(wrapper.BotActivity.ActivityText,
              (DiscordActivityType)wrapper.BotActivity.ActivityType,
                (DiscordUserStatus)wrapper.BotActivity.StatusType);
          }
          catch (Exception)
          {
            Logger.Error("Could not update bot activity");
          }

          break;
        case MessageWrapper.MessageOneofCase.ChatMessage:
          try
          {
            foreach (string content in SplitString(wrapper.ChatMessage.Content, 1999))
            {
              MessageScheduler.QueueMessage(wrapper.ChatMessage.ChannelID, content);
            }
          }
          catch (Exception)
          {
            Logger.Error("Could not send message in text channel '" + wrapper.ChatMessage.ChannelID + "'");
          }

          break;
        case MessageWrapper.MessageOneofCase.UserQuery:
          try
          {
            await DiscordAPI.GetPlayerRoles(wrapper.UserQuery.DiscordID, wrapper.UserQuery.SteamIDOrIP);
          }
          catch (Exception)
          {
            Logger.Error("Could not fetch discord roles for '" + wrapper.UserQuery.DiscordID + "'");
          }

          break;
        case MessageWrapper.MessageOneofCase.EmbedMessage:
          try
          {
            if (wrapper.EmbedMessage.InteractionID == 0)
            {
              await DiscordAPI.SendMessage(wrapper.EmbedMessage.ChannelID, Utilities.GetDiscordEmbed(wrapper.EmbedMessage));
            }
            else
            {
              await DiscordAPI.SendInteractionResponse(wrapper.EmbedMessage.InteractionID,
                                                       wrapper.EmbedMessage.ChannelID,
                                                       Utilities.GetDiscordEmbed(wrapper.EmbedMessage));
            }
          }
          catch (Exception e)
          {
            Logger.Error("Could not send embed in text channel '" + wrapper.EmbedMessage.ChannelID + "'", e);
          }

          break;
        case MessageWrapper.MessageOneofCase.PaginatedMessage:
          try
          {
            if (wrapper.PaginatedMessage.InteractionID == 0)
            {
              await DiscordAPI.SendPaginatedMessage(wrapper.PaginatedMessage.ChannelID,
                                                    wrapper.PaginatedMessage.UserID,
                                                    Utilities.GetPaginatedMessage(wrapper.PaginatedMessage));
            }
            else
            {
              await DiscordAPI.SendPaginatedResponse(wrapper.PaginatedMessage.InteractionID,
                                                     wrapper.PaginatedMessage.ChannelID,
                                                     wrapper.PaginatedMessage.UserID,
                                                     Utilities.GetPaginatedMessage(wrapper.PaginatedMessage));
            }
          }
          catch (Exception e)
          {
            Logger.Error("Could not send paginated message in text channel '" + wrapper?.PaginatedMessage?.ChannelID + "'", e);
          }

          break;
        case MessageWrapper.MessageOneofCase.BanCommand:
        case MessageWrapper.MessageOneofCase.ConsoleCommand:
        case MessageWrapper.MessageOneofCase.KickCommand:
        case MessageWrapper.MessageOneofCase.KickallCommand:
        case MessageWrapper.MessageOneofCase.ListCommand:
        case MessageWrapper.MessageOneofCase.ListRankedCommand:
        case MessageWrapper.MessageOneofCase.ListSyncedCommand:
        case MessageWrapper.MessageOneofCase.PlayerInfoCommand:
        case MessageWrapper.MessageOneofCase.SyncRoleCommand:
        case MessageWrapper.MessageOneofCase.UnbanCommand:
        case MessageWrapper.MessageOneofCase.UnsyncRoleCommand:
        case MessageWrapper.MessageOneofCase.UserInfo:
          Logger.Warn("Received packet meant for plugin: " + JsonFormatter.Default.Format(wrapper));
          break;
        case MessageWrapper.MessageOneofCase.None:
        default:
          Logger.Warn("Unknown packet received: " + JsonFormatter.Default.Format(wrapper));
          break;
      }
    }

    public static async Task SendMessage(MessageWrapper message, SlashCommandContext command)
    {
      try
      {
        Logger.Debug("Sent packet '" + JsonFormatter.Default.Format(message) + "' to plugin.");
        message.WriteDelimitedTo(networkStream);
      }
      catch (Exception)
      {
        if (command != null)
        {
          DiscordEmbed error = new DiscordEmbedBuilder
          {
            Color = DiscordColor.Red,
            Description = "Error communicating with server. Is it running?"
          };
          await command.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(error));
        }
      }
    }

    public static bool IsConnected()
    {
      if (clientSocket == null)
      {
        return false;
      }

      try
      {
        return !((clientSocket.Poll(1000, SelectMode.SelectRead) && (clientSocket.Available == 0)) || !clientSocket.Connected);
      }
      catch (ObjectDisposedException e)
      {
        Logger.Error("TCP client was unexpectedly closed.", e);
        return false;
      }
    }

    private static IEnumerable<string> SplitString(string str, int size)
    {
      for (int i = 0; i < str.Length; i += size)
      {
        yield return str.Substring(i, Math.Min(size, str.Length - i));
      }
    }
  }
}