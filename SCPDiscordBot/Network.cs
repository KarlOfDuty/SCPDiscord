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
using DSharpPlus.SlashCommands;

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
						Update();
					}
					else
					{
						DiscordAPI.SetDisconnectedActivity();
						Logger.Log("Listening on " + ipAddress + ":" + ConfigParser.config.plugin.port, LogID.NETWORK);
						clientSocket = listenerSocket.Accept();
						networkStream = new NetworkStream(clientSocket, true);
						Logger.Log("Plugin connected.", LogID.NETWORK);
					}
				}
				catch (Exception e)
				{
					Logger.Error("Network error caught, if this happens a lot try using the 'scpd_rc' command." + e, LogID.NETWORK);
				}
			}
		}

		private static async void Update()
		{
			MessageWrapper wrapper;
			try
			{
				wrapper = MessageWrapper.Parser.ParseDelimitedFrom(networkStream);
			}
			catch (Exception)
			{
				Logger.Error("Couldn't parse incoming packet!", LogID.NETWORK);
				return;
			}

			Logger.Debug("Incoming packet: " + JsonFormatter.Default.Format(wrapper), LogID.NETWORK);

			switch (wrapper.MessageCase)
			{
				case MessageWrapper.MessageOneofCase.BotActivity:
					try
					{
						DiscordAPI.SetActivity(wrapper.BotActivity.ActivityText, (ActivityType)wrapper.BotActivity.ActivityType, (UserStatus)wrapper.BotActivity.StatusType);
					}
					catch (Exception)
					{
						Logger.Error("Could not update bot activity", LogID.DISCORD);
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
						Logger.Error("Could not send message in text channel '" + wrapper.ChatMessage.ChannelID + "'", LogID.DISCORD);
					}
					break;
				case MessageWrapper.MessageOneofCase.UserQuery:
					try
					{
						DiscordAPI.GetPlayerRoles(wrapper.UserQuery.DiscordID, wrapper.UserQuery.SteamIDOrIP);
					}
					catch (Exception)
					{
						Logger.Error("Could not fetch discord roles for '" + wrapper.UserQuery.DiscordID + "'", LogID.DISCORD);
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
							await DiscordAPI.SendInteractionResponse(wrapper.EmbedMessage.InteractionID, wrapper.EmbedMessage.ChannelID, Utilities.GetDiscordEmbed(wrapper.EmbedMessage));
						}
					}
					catch (Exception e)
					{
						Logger.Error("Could not send embed in text channel '" + wrapper.EmbedMessage.ChannelID + "' Exception: " + e, LogID.DISCORD);
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
						Logger.Error("Could not send paginated message in text channel '" + wrapper?.PaginatedMessage?.ChannelID + "' Exception: " + e, LogID.DISCORD);
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
					Logger.Warn("Received packet meant for plugin: " + JsonFormatter.Default.Format(wrapper), LogID.NETWORK);
					break;
				case MessageWrapper.MessageOneofCase.None:
				default:
					Logger.Warn("Unknown packet received: " + JsonFormatter.Default.Format(wrapper), LogID.NETWORK);
					break;
			}
		}

		public static async Task SendMessage(MessageWrapper message, InteractionContext interaction)
		{
			try
			{
				message.WriteDelimitedTo(networkStream);
			}
			catch (Exception)
			{
				if (interaction != null)
				{
					DiscordEmbed error = new DiscordEmbedBuilder
					{
						Color = DiscordColor.Red,
						Description = "Error communicating with server. Is it running?"
					};
					await interaction.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(error));
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
				Logger.Error("TCP client was unexpectedly closed.", LogID.NETWORK);
				Logger.Debug(e.ToString(), LogID.NETWORK);
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
