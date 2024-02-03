using Google.Protobuf;
using SCPDiscord.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using PluginAPI.Core;

namespace SCPDiscord
{
	// Separate class to run the thread
	public class StartNetworkSystem
	{
		public StartNetworkSystem(SCPDiscord plugin)
		{
			NetworkSystem.Run(plugin);
		}
	}

	public class ProcessMessageAsync
	{
		public ProcessMessageAsync(string messagePath, Dictionary<string, string> variables)
		{
			string processedMessage = Language.GetProcessedMessage(messagePath, variables);

			// Add time stamp
			if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
			{
				processedMessage = "[" + DateTime.Now.ToString(Config.GetString("settings.timestamp")) + "]: " + processedMessage;
			}

			foreach (ulong channelID in Config.GetChannelIDs(messagePath))
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
		public ProcessEmbedMessageAsync(EmbedMessage embed, string messagePath, Dictionary<string, string> variables)
		{
			string processedMessage = Language.GetProcessedMessage(messagePath, variables);
			embed.Description = processedMessage;

			// Add time stamp
			if (Config.GetString("settings.timestamp") != "off" && Config.GetString("settings.timestamp") != "")
			{
				embed.Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
			}

			foreach (ulong channelID in Config.GetChannelIDs(messagePath))
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
		private const int ACTIVITY_UPDATE_RATE_MS = 10000;
		private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		public static NetworkStream networkStream = null;
		private static readonly List<MessageWrapper> messageQueue = new List<MessageWrapper>();
		private static SCPDiscord plugin;
		private static Stopwatch activityUpdateTimer = new Stopwatch();

		private static Thread messageThread;

		public static void Run(SCPDiscord pl)
		{
			plugin = pl;
			while (!Config.ready || !Language.ready)
			{
				Thread.Sleep(1000);
			}

			while (!plugin.shutdown)
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
				Logger.DebugWarn("Could not send all messages.");
			}
		}

		/// Connection functions //////////////////////////
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
				Logger.DebugError(e.ToString());
				return false;
			}
		}

		private static void Connect()
		{
			Logger.Info("Attempting Bot Connection...");
			Logger.Debug("Your Bot IP: " + Config.GetString("bot.ip") + ". Your Bot Port: " + Config.GetInt("bot.port") + ".");

			while (!IsConnected())
			{
				try
				{
					if (socket != null && socket.IsBound)
					{
						//socket.Shutdown(SocketShutdown.Both);
						messageThread?.Abort();
						socket.Close();
					}

					socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					socket.Connect(Config.GetString("bot.ip"), Config.GetInt("bot.port"));
					messageThread = new Thread(() => new BotListener(plugin));
					messageThread.Start();

					networkStream = new NetworkStream(socket);

					Logger.Info("Connected to Discord bot.");

					EmbedMessage embed = new EmbedMessage
					{
						Colour = EmbedMessage.Types.DiscordColour.Green
					};

					plugin.SendEmbedWithMessage("messages.connectedtobot", embed);
				}
				catch (SocketException e)
				{
					Logger.Error("Error occured while connecting to discord bot server: " + e.Message.Trim());
					Logger.DebugError(e.ToString());
					Thread.Sleep(5000);
				}
				catch (ObjectDisposedException e)
				{
					Logger.Error("TCP client was unexpectedly closed.");
					Logger.DebugError(e.ToString());
					Thread.Sleep(5000);
				}
				catch (ArgumentOutOfRangeException e)
				{
					Logger.Error("Invalid port.");
					Logger.DebugError(e.ToString());
					Thread.Sleep(5000);
				}
				catch (ArgumentNullException e)
				{
					Logger.Error("IP address is null.");
					Logger.DebugError(e.ToString());
					Thread.Sleep(5000);
				}
			}
		}

		public static void Disconnect()
		{
			socket.Disconnect(false);
		}
		/// ///////////////////////////////////////////////

		/// Message functions /////////////////////////////
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
				Logger.DebugWarn("Error sending message '" + message.MessageCase + "' to bot: Not connected.");
				return false;
			}

			// Try to send the message to the bot
			try
			{
				message.WriteDelimitedTo(networkStream);
				Logger.Debug("Sent message '" + message.MessageCase + "' to bot.");
				return true;
			}
			catch (Exception e)
			{
				Logger.Error("Error sending message '" + message.MessageCase + "' to bot.");
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
					if (Config.GetChannelIDs("channelsettings.filterips").Contains(message.EmbedMessage.ChannelID))
					{
						foreach (Player player in Player.GetPlayers())
						{
							message.EmbedMessage.Description = message.EmbedMessage.Description.Replace(player.IpAddress, new string('#', player.IpAddress.Length));
						}
					}
					if (Config.GetChannelIDs("channelsettings.filtersteamids").Contains(message.EmbedMessage.ChannelID))
					{
						foreach (Player player in Player.GetPlayers())
						{
							message.EmbedMessage.Description = message.EmbedMessage.Description.Replace(player.GetParsedUserID(), "Player " + player.PlayerId);
						}
					}
					break;
				case MessageWrapper.MessageOneofCase.ChatMessage:
					if (Config.GetChannelIDs("channelsettings.filterips").Contains(message.ChatMessage.ChannelID))
					{
						foreach (Player player in Player.GetPlayers())
						{
							message.ChatMessage.Content = message.ChatMessage.Content.Replace(player.IpAddress, new string('#', player.IpAddress.Length));
						}
					}
					if (Config.GetChannelIDs("channelsettings.filtersteamids").Contains(message.ChatMessage.ChannelID))
					{
						foreach (Player player in Player.GetPlayers())
						{
							message.ChatMessage.Content = message.ChatMessage.Content.Replace(player.GetParsedUserID(), "Player " + player.PlayerId);
						}
					}
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
				default:
					break;
			}

			messageQueue.Add(message);
		}

		/// ///////////////////////////////////////////////

		/// Status refreshing //////////////////////

		private static void RefreshBotStatus()
		{
			if (activityUpdateTimer.ElapsedMilliseconds < ACTIVITY_UPDATE_RATE_MS && activityUpdateTimer.IsRunning) return;

			activityUpdateTimer.Reset();
			activityUpdateTimer.Start();

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
