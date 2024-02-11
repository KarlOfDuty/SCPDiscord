using SCPDiscord.BotCommands;
using System;
using System.IO;
using System.Threading;

namespace SCPDiscord
{
	class BotListener
	{
		public BotListener()
		{
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
								Logger.Error("Connection to bot lost.");
							else
								Logger.Error("Couldn't parse incoming packet!\n" + e);
							return;
						}

						Logger.Debug("Incoming packet: " + Google.Protobuf.JsonFormatter.Default.Format(data));

						switch (data.MessageCase)
						{
							case Interface.MessageWrapper.MessageOneofCase.SyncRoleCommand:
								SCPDiscord.plugin.SendEmbedByID(RoleSync.AddPlayer(data.SyncRoleCommand));
								break;

							case Interface.MessageWrapper.MessageOneofCase.UnsyncRoleCommand:
								SCPDiscord.plugin.SendEmbedByID(RoleSync.RemovePlayer(data.UnsyncRoleCommand));
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
					Thread.Sleep(500);
				}
				catch (Exception ex)
				{
					Logger.Error("BotListener Error: " + ex);
				}
			}
		}
	}
}
