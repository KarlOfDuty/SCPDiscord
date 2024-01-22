using SCPDiscord.BotCommands;
using System;
using System.IO;
using System.Threading;

namespace SCPDiscord
{
	class BotListener
	{
		private readonly SCPDiscord plugin;
		public BotListener(SCPDiscord plugin)
		{
			this.plugin = plugin;
			while (true)
			{
				try
				{
					//Listen for connections
					if (NetworkSystem.IsConnected())
					{
						Interface.MessageWrapper data;
						try
						{
							data = Interface.MessageWrapper.Parser.ParseDelimitedFrom(NetworkSystem.networkStream);
						}
						catch (Exception e)
						{
							if (e is IOException)
								plugin.Error("Connection to bot lost.");
							else
								plugin.Error("Couldn't parse incoming packet!\n" + e);
							return;
						}

						plugin.Debug("Incoming packet: " + Google.Protobuf.JsonFormatter.Default.Format(data));

						switch (data.MessageCase)
						{
							case Interface.MessageWrapper.MessageOneofCase.SyncRoleCommand:
								plugin.SendEmbedByID(plugin.roleSync.AddPlayer(data.SyncRoleCommand));
								break;

							case Interface.MessageWrapper.MessageOneofCase.UnsyncRoleCommand:
								plugin.SendEmbedByID(plugin.roleSync.RemovePlayer(data.UnsyncRoleCommand));
								break;

							case Interface.MessageWrapper.MessageOneofCase.ConsoleCommand:
								plugin.sync.ScheduleDiscordCommand(data.ConsoleCommand);
								break;

							case Interface.MessageWrapper.MessageOneofCase.UserInfo:
								plugin.roleSync.ReceiveQueryResponse(data.UserInfo);
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

							case Interface.MessageWrapper.MessageOneofCase.MuteCommand:
								MuteCommand.Execute(data.MuteCommand);
								break;

							case Interface.MessageWrapper.MessageOneofCase.BotActivity:
							case Interface.MessageWrapper.MessageOneofCase.ChatMessage:
							case Interface.MessageWrapper.MessageOneofCase.UserQuery:
							case Interface.MessageWrapper.MessageOneofCase.PaginatedMessage:
							case Interface.MessageWrapper.MessageOneofCase.EmbedMessage:
								plugin.Error("Received packet meant for bot: " + Google.Protobuf.JsonFormatter.Default.Format(data));
								break;

							case Interface.MessageWrapper.MessageOneofCase.None:
							default:
								plugin.Warn("Unknown packet received: " + Google.Protobuf.JsonFormatter.Default.Format(data));
								break;
						}
					}
					Thread.Sleep(500);
				}
				catch (Exception ex)
				{
					plugin.Error("BotListener Error: " + ex);
				}
			}
		}
	}
}
