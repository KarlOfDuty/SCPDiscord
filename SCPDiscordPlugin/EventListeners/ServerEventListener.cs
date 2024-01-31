using System;
using System.Collections.Generic;
using CommandSystem;
using LiteNetLib;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using RemoteAdmin;

namespace SCPDiscord.EventListeners
{
	internal class ServerEventListener
	{
		private readonly SCPDiscord plugin;

		public ServerEventListener(SCPDiscord plugin)
		{
			this.plugin = plugin;
		}

		[PluginEvent]
		public void OnPlayerBanned(PlayerBannedEvent ev)
		{
			if (!(ev.Player is Player player)) return;

			if (ev.Issuer != null)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration", Utilities.SecondsToCompoundTime(ev.Duration) },
					{ "reason",   ev.Reason }
				};
				variables.AddPlayerVariables(player, "player");
				variables.AddPlayerVariables(ev.Issuer, "issuer");

				if (ev.Duration == 0)
				{
					plugin.SendMessage("messages.onkick.player", variables);
				}
				else
				{
					plugin.SendMessage("messages.onban.player", variables);
				}
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration", Utilities.SecondsToCompoundTime(ev.Duration) },
					{ "reason",   ev.Reason }
				};
				variables.AddPlayerVariables(player, "player");

				if (ev.Duration == 0)
				{
					plugin.SendMessage("messages.onkick.server", variables);
				}
				else
				{
					plugin.SendMessage("messages.onban.server", variables);
				}
			}
		}

		[PluginEvent]
		public void OnPlayerKicked(PlayerKickedEvent ev)
		{
			if (ev.Player == null) return;

			if (ev.Issuer is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
			{
				Player issuer = Player.Get(playerSender.ReferenceHub);
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "reason", ev.Reason}
				};
				variables.AddPlayerVariables(issuer, "issuer");
				variables.AddPlayerVariables(ev.Player, "player");

				plugin.SendMessage("messages.onkick.player", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "reason", ev.Reason}
				};
				variables.AddPlayerVariables(ev.Player, "player");

				plugin.SendMessage("messages.onkick.server", variables);
			}
		}

		[PluginEvent]
		public void OnBanIssued(BanIssuedEvent ev)
		{
			if (ev.BanType == BanHandler.BanType.IP)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration",    Utilities.TicksToCompoundTime(ev.BanDetails.Expires - ev.BanDetails.IssuanceTime + 1000000) },
					{ "expirytime",  new DateTime(ev.BanDetails.Expires).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "issuedtime",  new DateTime(ev.BanDetails.IssuanceTime).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "reason",      ev.BanDetails.Reason       },
					{ "player-ip",   ev.BanDetails.Id           },
					{ "player-name", ev.BanDetails.OriginalName },
					{ "issuer-name", ev.BanDetails.Issuer       },
				};
				plugin.SendMessage("messages.onbanissued.ip", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration",      Utilities.TicksToCompoundTime(ev.BanDetails.Expires - ev.BanDetails.IssuanceTime + 1000000) },
					{ "expirytime",    new DateTime(ev.BanDetails.Expires).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "issuedtime",    new DateTime(ev.BanDetails.IssuanceTime).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "reason",        ev.BanDetails.Reason       },
					{ "player-userid", ev.BanDetails.Id           },
					{ "player-name",   ev.BanDetails.OriginalName },
					{ "issuer-name",   ev.BanDetails.Issuer       },
				};
				plugin.SendMessage("messages.onbanissued.userid", variables);
			}
		}

		[PluginEvent]
		public void OnBanUpdated(BanUpdatedEvent ev)
		{
			if (ev.BanType == BanHandler.BanType.IP)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration",    Utilities.TicksToCompoundTime(ev.BanDetails.Expires - ev.BanDetails.IssuanceTime + 1000000) },
					{ "expirytime",  new DateTime(ev.BanDetails.Expires).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "issuedtime",  new DateTime(ev.BanDetails.IssuanceTime).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "reason",      ev.BanDetails.Reason       },
					{ "player-ip",   ev.BanDetails.Id           },
					{ "player-name", ev.BanDetails.OriginalName },
					{ "issuer-name", ev.BanDetails.Issuer       },
				};
				plugin.SendMessage("messages.onbanupdated.ip", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration",      Utilities.TicksToCompoundTime(ev.BanDetails.Expires - ev.BanDetails.IssuanceTime + 1000000) },
					{ "expirytime",    new DateTime(ev.BanDetails.Expires).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "issuedtime",    new DateTime(ev.BanDetails.IssuanceTime).ToString("yyyy-MM-dd HH:mm:ss") },
					{ "reason",        ev.BanDetails.Reason       },
					{ "player-userid", ev.BanDetails.Id           },
					{ "player-name",   ev.BanDetails.OriginalName },
					{ "issuer-name",   ev.BanDetails.Issuer       },
				};
				plugin.SendMessage("messages.onbanupdated.userid", variables);
			}
		}

		[PluginEvent]
		public void OnBanRevoked(BanRevokedEvent ev)
		{
			if (ev.BanType == BanHandler.BanType.IP)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "ip", ev.Id },
				};
				plugin.SendMessage("messages.onbanrevoked.ip", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "userid", ev.Id },
				};
				plugin.SendMessage("messages.onbanrevoked.userid", variables);
			}
		}

		[PluginEvent]
		public void OnPlayerMuted(PlayerMutedEvent ev)
		{
			if (ev?.Player.UserId == null) return;

			if (ev.Issuer != null)
			{
				Dictionary<string, string> variables = new Dictionary<string, string> {};
				variables.AddPlayerVariables(ev.Player, "player");
				variables.AddPlayerVariables(ev.Issuer, "issuer");

				if (ev.IsIntercom)
				{
					plugin.SendMessage("messages.onplayermuted.player.intercom", variables);
				}
				else
				{
					plugin.SendMessage("messages.onplayermuted.player.standard", variables);
				}
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string> {};
				variables.AddPlayerVariables(ev.Player, "player");

				if (ev.IsIntercom)
				{
					plugin.SendMessage("messages.onplayermuted.server.intercom", variables);
				}
				else
				{
					plugin.SendMessage("messages.onplayermuted.server.standard", variables);
				}
			}
		}

		[PluginEvent]
		public void OnPlayerUnmuted(PlayerUnmutedEvent ev)
		{
			if (ev.Player == null) return;

			if (ev.Issuer != null)
			{
				Dictionary<string, string> variables = new Dictionary<string, string> {};
				variables.AddPlayerVariables(ev.Issuer, "issuer");
				variables.AddPlayerVariables(ev.Player, "player");

				if (ev.IsIntercom)
				{
					plugin.SendMessage("messages.onplayerunmuted.player.intercom", variables);
				}
				else
				{
					plugin.SendMessage("messages.onplayerunmuted.player.standard", variables);
				}
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string> {};
				variables.AddPlayerVariables(ev.Player, "player");

				if (ev.IsIntercom)
				{
					plugin.SendMessage("messages.onplayerunmuted.server.intercom", variables);
				}
				else
				{
					plugin.SendMessage("messages.onplayerunmuted.server.standard", variables);
				}
			}
		}

		[PluginEvent]
		public void OnRemoteAdminCommand(RemoteAdminCommandExecutedEvent ev)
		{
			if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
			{
				Player player = Player.Get(playerSender.ReferenceHub);
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command",      (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
					{ "result",        ev.Result.ToString() },
					{ "returnmessage", ev.Response }
				};
				variables.AddPlayerVariables(player, "player");
				plugin.SendMessage("messages.onexecutedcommand.remoteadmin.player", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command",       (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
					{ "result",        ev.Result.ToString() },
					{ "returnmessage", ev.Response }
				};
				plugin.SendMessage("messages.onexecutedcommand.remoteadmin.server", variables);
			}
		}

		[PluginEvent]
		public void OnGameConsoleCommand(PlayerGameConsoleCommandExecutedEvent ev)
		{
			if (ev.Player != null && ev.Player.PlayerId != Server.Instance.PlayerId)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command",      (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
					{ "returnmessage", ev.Response }
				};
				variables.AddPlayerVariables(ev.Player, "player");
				plugin.SendMessage("messages.onexecutedcommand.game.player", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command",      (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
					{ "returnmessage", ev.Response }
				};
				plugin.SendMessage("messages.onexecutedcommand.game.server", variables);
			}
		}

		[PluginEvent]
		public void OnConsoleCommand(ConsoleCommandExecutedEvent ev)
		{
			if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
			{
				Player player = Player.Get(playerSender.ReferenceHub);
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command",      (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
					{ "result",        ev.Result.ToString()                        },
					{ "returnmessage", ev.Response                                 }
				};
				variables.AddPlayerVariables(player, "player");
				plugin.SendMessage("messages.onexecutedcommand.console.player", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command",      (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
					{ "result",        ev.Result.ToString() },
					{ "returnmessage", ev.Response }
				};
				plugin.SendMessage("messages.onexecutedcommand.console.server", variables);
			}
		}

		[PluginEvent]
		public void OnRemoteAdminCommand(RemoteAdminCommandEvent ev)
		{
			if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
			{
				Player player = Player.Get(playerSender.ReferenceHub);
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() }
				};
				variables.AddPlayerVariables(player, "player");
				plugin.SendMessage("messages.oncallcommand.remoteadmin.player", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() }
				};
				plugin.SendMessage("messages.oncallcommand.remoteadmin.server", variables);
			}
		}

		[PluginEvent]
		public void OnGameConsoleCommand(PlayerGameConsoleCommandEvent ev)
		{
			if (ev.Player != null && ev.Player.PlayerId != Server.Instance.PlayerId)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() }
				};
				variables.AddPlayerVariables(ev.Player, "player");
				plugin.SendMessage("messages.oncallcommand.game.player", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() }
				};
				plugin.SendMessage("messages.oncallcommand.game.server", variables);
			}
		}

		[PluginEvent]
		public void OnConsoleCommand(ConsoleCommandEvent ev)
		{
			if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
			{
				Player player = Player.Get(playerSender.ReferenceHub);
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command",   (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
				};
				variables.AddPlayerVariables(player, "player");
				plugin.SendMessage("messages.oncallcommand.console.player", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() }
				};
				plugin.SendMessage("messages.oncallcommand.console.server", variables);
			}
		}

		[PluginEvent]
		public void OnRoundStart(RoundStartEvent ev)
		{
			plugin.SendMessage("messages.onroundstart");
			plugin.roundStarted = true;
		}

		[PluginEvent]
		public void OnConnect(PlayerPreauthEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress", ev.IpAddress                    },
				{ "userid",    ev.UserId.Replace("@steam", "") },
				{ "jointype",  ev.CentralFlags.ToString()      },
				{ "region",    ev.Region                       }
			};
			plugin.SendMessage("messages.onconnect", variables);
		}

		[PluginEvent]
		public void OnRoundEnd(RoundEndEvent ev)
		{
			if (plugin.roundStarted && new TimeSpan(DateTime.Now.Ticks - Statistics.CurrentRound.StartTimestamp.Ticks).TotalSeconds > 60)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "duration",          (new TimeSpan(DateTime.Now.Ticks - Statistics.CurrentRound.StartTimestamp.Ticks).TotalSeconds / 60).ToString("0") },
					{ "leadingteam",        ev.LeadingTeam.ToString()                            },
					{ "dclassalive",        Statistics.CurrentRound.ClassDAlive.ToString()       },
					{ "dclassdead",         Statistics.CurrentRound.ClassDDead.ToString()        },
					{ "dclassescaped",      Statistics.CurrentRound.ClassDEscaped.ToString()     },
					{ "dclassstart",        Statistics.CurrentRound.ClassDStart.ToString()       },
					{ "mtfalive",           Statistics.CurrentRound.MtfAndGuardsAlive.ToString() },
					{ "mtfdead",            Statistics.CurrentRound.MtfAndGuardsDead.ToString()  },
					{ "mtfstart",           Statistics.CurrentRound.MtfAndGuardsStart.ToString() },
					{ "scientistsalive",    Statistics.CurrentRound.ScientistsAlive.ToString()   },
					{ "scientistsdead",     Statistics.CurrentRound.ScientistsDead.ToString()    },
					{ "scientistsescaped",  Statistics.CurrentRound.ScientistsEscaped.ToString() },
					{ "scientistsstart",    Statistics.CurrentRound.ScientistsStart.ToString()   },
					{ "scpalive",           Statistics.CurrentRound.ScpsAlive.ToString()         },
					{ "scpdead",            Statistics.CurrentRound.ScpsDead.ToString()          },
					{ "scpkills",           Statistics.CurrentRound.TotalScpKills.ToString()     },
					{ "scpstart",           Statistics.CurrentRound.ScpsStart.ToString()         },
					{ "warheaddetonated",   Statistics.CurrentRound.WarheadDetonated.ToString()  },
					{ "warheadkills",       Statistics.CurrentRound.WarheadKills.ToString()      },
					{ "zombiesalive",       Statistics.CurrentRound.ZombiesAlive.ToString()      },
					{ "zombieschanged",     Statistics.CurrentRound.ZombiesChanged.ToString()    }
				};
				plugin.SendMessage("messages.onroundend", variables);
			}
			plugin.roundStarted = false;
		}

		[PluginEvent]
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			plugin.SendMessage("messages.onwaitingforplayers");
		}

		[PluginEvent]
		public void OnRoundRestart(RoundRestartEvent ev)
		{
			plugin.SendMessage("messages.onroundrestart");
		}

		[PluginEvent]
		public void OnPlayerCheaterReport(PlayerCheaterReportEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "reason", ev.Reason }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			variables.AddPlayerVariables(ev.Target, "target");
			plugin.SendMessage("messages.onplayercheaterreport", variables);
		}

		[PluginEvent]
		public void OnPlayerReport(PlayerReportEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "reason", ev.Reason }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			variables.AddPlayerVariables(ev.Target, "target");
			plugin.SendMessage("messages.onplayerreport", variables);
		}
	}
}