using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;

namespace SCPDiscord.EventListeners
{
  internal class ServerEventListener : CustomEventsHandler
  {
    private readonly SCPDiscord plugin;

    public ServerEventListener(SCPDiscord plugin)
    {
      this.plugin = plugin;
    }

    public override void OnPlayerBanned(PlayerBannedEventArgs ev)
    {
      if (!(ev.Player is Player player))
      {
        return;
      }

      if (ev.Issuer != null && ev.Issuer.PlayerId != Player.Host?.PlayerId)
      {
        Dictionary<string, string> variables = new()
        {
          { "duration", Utilities.SecondsToCompoundTime(ev.Duration) },
          { "reason",   ev.Reason }
        };
        variables.AddPlayerVariables(player, "player");
        variables.AddPlayerVariables(ev.Issuer, "issuer");

        if (ev.Duration == 0)
        {
          SCPDiscord.SendMessage("messages.onkick.player", variables);
        }
        else
        {
          SCPDiscord.SendMessage("messages.onban.player", variables);
        }
      }
      else
      {
        Dictionary<string, string> variables = new()
        {
          { "duration", Utilities.SecondsToCompoundTime(ev.Duration) },
          { "reason",   ev.Reason }
        };
        variables.AddPlayerVariables(player, "player");

        if (ev.Duration == 0)
        {
          SCPDiscord.SendMessage("messages.onkick.server", variables);
        }
        else
        {
          SCPDiscord.SendMessage("messages.onban.server", variables);
        }
      }
    }

    public override void OnPlayerKicked(PlayerKickedEventArgs ev)
    {
      if (ev.Player == null)
      {
        return;
      }

      if (ev.Issuer != null && ev.Issuer.PlayerId != Player.Host?.PlayerId)
      {
        Dictionary<string, string> variables = new()
        {
          { "reason", ev.Reason}
        };
        variables.AddPlayerVariables(ev.Issuer, "issuer");
        variables.AddPlayerVariables(ev.Player, "player");

        SCPDiscord.SendMessage("messages.onkick.player", variables);
      }
      else
      {
        Dictionary<string, string> variables = new()
        {
          { "reason", ev.Reason}
        };
        variables.AddPlayerVariables(ev.Player, "player");

        SCPDiscord.SendMessage("messages.onkick.server", variables);
      }
    }

    public override void OnServerBanIssued(BanIssuedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "duration",    Utilities.TicksToCompoundTime(ev.BanDetails.Expires - ev.BanDetails.IssuanceTime + 1000000) },
        { "expirytime",  new DateTime(ev.BanDetails.Expires).ToString("yyyy-MM-dd HH:mm:ss") },
        { "issuedtime",  new DateTime(ev.BanDetails.IssuanceTime).ToString("yyyy-MM-dd HH:mm:ss") },
        { "reason",      ev.BanDetails.Reason       },
        { "player-name", ev.BanDetails.OriginalName },
        { "issuer-name", ev.BanDetails.Issuer       },
      };

      if (ev.BanType == BanHandler.BanType.IP)
      {
        variables.Add("player-ip", ev.BanDetails.Id);
        SCPDiscord.SendMessage("messages.onbanissued.ip", variables);
      }
      else
      {
        variables.Add("player-userid", ev.BanDetails.Id);
        SCPDiscord.SendMessage("messages.onbanissued.userid", variables);
      }
    }

    public override void OnServerBanUpdated(BanUpdatedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "duration",    Utilities.TicksToCompoundTime(ev.BanDetails.Expires - ev.BanDetails.IssuanceTime + 1000000) },
        { "expirytime",  new DateTime(ev.BanDetails.Expires).ToString("yyyy-MM-dd HH:mm:ss") },
        { "issuedtime",  new DateTime(ev.BanDetails.IssuanceTime).ToString("yyyy-MM-dd HH:mm:ss") },
        { "reason",      ev.BanDetails.Reason       },
        { "player-ip",   ev.BanDetails.Id           },
        { "player-name", ev.BanDetails.OriginalName },
        { "issuer-name", ev.BanDetails.Issuer       },
      };

      if (ev.BanType == BanHandler.BanType.IP)
      {
        variables.Add("player-ip", ev.BanDetails.Id);
        SCPDiscord.SendMessage("messages.onbanupdated.ip", variables);
      }
      else
      {
        variables.Add("player-userid", ev.BanDetails.Id);
        SCPDiscord.SendMessage("messages.onbanupdated.userid", variables);
      }
    }

    // TODO: Add more details
    public override void OnServerBanRevoked(BanRevokedEventArgs ev)
    {
      if (ev.BanType == BanHandler.BanType.IP)
      {
        Dictionary<string, string> variables = new()
        {
          { "ip", ev.BanDetails.Id },
        };
        SCPDiscord.SendMessage("messages.onbanrevoked.ip", variables);
      }
      else
      {
        Dictionary<string, string> variables = new()
        {
          { "userid", ev.BanDetails.Id },
        };
        SCPDiscord.SendMessage("messages.onbanrevoked.userid", variables);
      }
    }

    public override void OnPlayerMuted(PlayerMutedEventArgs ev)
    {
      if (ev?.Player.UserId == null)
      {
        return;
      }

      if (ev.Issuer != null && ev.Issuer.PlayerId != Player.Host?.PlayerId)
      {
        Dictionary<string, string> variables = new();
        variables.AddPlayerVariables(ev.Player, "player");
        variables.AddPlayerVariables(ev.Issuer, "issuer");

        SCPDiscord.SendMessage(ev.IsIntercom ? "messages.onplayermuted.player.intercom"
                                         : "messages.onplayermuted.player.standard", variables);
      }
      else
      {
        Dictionary<string, string> variables = new();
        variables.AddPlayerVariables(ev.Player, "player");

        SCPDiscord.SendMessage(ev.IsIntercom ? "messages.onplayermuted.server.intercom"
                                         : "messages.onplayermuted.server.standard", variables);
      }
    }

    public override void OnPlayerUnmuted(PlayerUnmutedEventArgs ev)
    {
      if (ev.Player == null)
      {
        return;
      }

      if (ev.Issuer != null && ev.Issuer.PlayerId != Player.Host?.PlayerId)
      {
        Dictionary<string, string> variables = new();
        variables.AddPlayerVariables(ev.Issuer, "issuer");
        variables.AddPlayerVariables(ev.Player, "player");

        SCPDiscord.SendMessage(ev.IsIntercom ? "messages.onplayerunmuted.player.intercom"
                                         : "messages.onplayerunmuted.player.standard", variables);
      }
      else
      {
        Dictionary<string, string> variables = new();
        variables.AddPlayerVariables(ev.Player, "player");

        SCPDiscord.SendMessage(ev.IsIntercom ? "messages.onplayerunmuted.server.intercom"
                                         : "messages.onplayerunmuted.server.standard", variables);
      }
    }

    // TODO: Seem to be merged into one
    /*public void OnRemoteAdminCommand(RemoteAdminResponse ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "command",       (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
        { "result",        ev.Result.ToString() },
        { "returnmessage", ev.Response }
      };

      if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
      {
        variables.AddPlayerVariables(Player.Get(playerSender.ReferenceHub), "player");
        SCPDiscord.SendMessage("messages.onexecutedcommand.remoteadmin.player", variables);
      }
      else
      {
        SCPDiscord.SendMessage("messages.onexecutedcommand.remoteadmin.server", variables);
      }
    }

    public void OnGameConsoleCommand(PlayerGameConsoleCommandExecutedEvent ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "command",      (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
        { "returnmessage", ev.Response }
      };

      if (ev.Player != null && ev.Player.PlayerId != Server.Instance.PlayerId)
      {
        variables.AddPlayerVariables(ev.Player, "player");
        SCPDiscord.SendMessage("messages.onexecutedcommand.game.player", variables);
      }
      else
      {
        SCPDiscord.SendMessage("messages.onexecutedcommand.game.server", variables);
      }
    }

    public void OnConsoleCommand(ConsoleCommandExecutedEvent ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
            {
              { "command",      (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
              { "result",        ev.Result.ToString()                        },
              { "returnmessage", ev.Response                                 }
            };

      if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
      {
        variables.AddPlayerVariables(Player.Get(playerSender.ReferenceHub), "player");
        SCPDiscord.SendMessage("messages.onexecutedcommand.console.player", variables);
      }
      else
      {
        SCPDiscord.SendMessage("messages.onexecutedcommand.console.server", variables);
      }
    }

    public void OnRemoteAdminCommand(RemoteAdminCommandEvent ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() }
      };

      if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
      {
        variables.AddPlayerVariables(Player.Get(playerSender.ReferenceHub), "player");
        SCPDiscord.SendMessage("messages.oncallcommand.remoteadmin.player", variables);
      }
      else
      {
        SCPDiscord.SendMessage("messages.oncallcommand.remoteadmin.server", variables);
      }
    }

    public void OnGameConsoleCommand(PlayerGameConsoleCommandEvent ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() }
      };

      if (ev.Player != null && ev.Player.PlayerId != Server.Instance.PlayerId)
      {
        variables.AddPlayerVariables(ev.Player, "player");
        SCPDiscord.SendMessage("messages.oncallcommand.game.player", variables);
      }
      else
      {
        SCPDiscord.SendMessage("messages.oncallcommand.game.server", variables);
      }
    }

    public void OnConsoleCommand(ConsoleCommandEvent ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "command", (ev.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
      };

      if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
      {
        variables.AddPlayerVariables(Player.Get(playerSender.ReferenceHub), "player");
        SCPDiscord.SendMessage("messages.oncallcommand.console.player", variables);
      }
      else
      {
        SCPDiscord.SendMessage("messages.oncallcommand.console.server", variables);
      }
    }*/

    public override void OnServerRoundStarted()
    {
      SCPDiscord.SendMessage("messages.onroundstart");
      plugin.roundStarted = true;
    }

    public override void OnPlayerPreAuthenticated(PlayerPreAuthenticatedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "ipaddress", ev.IpAddress                    },
        { "userid",    ev.UserId.Replace("@steam", "") },
        { "jointype",  ev.Flags.ToString()             }, // TODO: This cant be right
        { "region",    ev.Region                       }
      };
      SCPDiscord.SendMessage("messages.onconnect", variables);
    }

    /*public override void OnServerRoundEnded(RoundEndedEventArgs ev)
    {
      if (plugin.roundStarted && RoundStart.RoundLength.TotalSeconds > 60)
      {
        Dictionary<string, string> variables = new()
        {
          { "duration",          (RoundStart.RoundLength.TotalSeconds / 60).ToString("0") },
          { "leadingteam",        ev.LeadingTeam.ToString()                            },
          { "dclassalive",        Statistics.CurrentRound.ClassDAlive.ToString()       },
          { "dclassdead",         Statistics.CurrentRound.ClassDDead.ToString()        },
          { "dclassescaped",      RoundSummary.EscapedClassD.ToString()                },
          { "dclassstart",        Statistics.CurrentRound.ClassDStart.ToString()       },
          { "mtfalive",           Statistics.CurrentRound.MtfAndGuardsAlive.ToString() },
          { "mtfdead",            Statistics.CurrentRound.MtfAndGuardsDead.ToString()  },
          { "mtfstart",           Statistics.CurrentRound.MtfAndGuardsStart.ToString() },
          { "scientistsalive",    Statistics.CurrentRound.ScientistsAlive.ToString()   },
          { "scientistsdead",     Statistics.CurrentRound.ScientistsDead.ToString()    },
          { "scientistsescaped",  RoundSummary.EscapedScientists.ToString()            },
          { "scientistsstart",    Statistics.CurrentRound.ScientistsStart.ToString()   },
          { "scpalive",           RoundSummary.SurvivingSCPs.ToString()                },
          { "scpdead",            Statistics.CurrentRound.ScpsDead.ToString()          },
          { "scpkills",           RoundSummary.KilledBySCPs.ToString()                 },
          { "scpstart",           Statistics.CurrentRound.ScpsStart.ToString()         },
          { "warheaddetonated",   Warhead.IsDetonated.ToString()                       },
          { "warheadkills",       Statistics.CurrentRound.WarheadKills.ToString()      },
          { "zombiesalive",       Statistics.CurrentRound.ZombiesAlive.ToString()      },
          { "zombieschanged",     RoundSummary.ChangedIntoZombies.ToString()           }
        };
        SCPDiscord.SendMessage("messages.onroundend", variables);
      }
      plugin.roundStarted = false;
    }*/

    public override void OnServerWaitingForPlayers()
    {
      SCPDiscord.SendMessage("messages.onwaitingforplayers");
    }

    public override void OnServerRoundRestarted()
    {
      SCPDiscord.SendMessage("messages.onroundrestart");
    }

    public override void OnPlayerReportedCheater(PlayerReportedCheaterEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "reason", ev.Reason }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      variables.AddPlayerVariables(ev.Target, "target");
      SCPDiscord.SendMessage("messages.onplayercheaterreport", variables);
    }

    public override void OnPlayerReportedPlayer(PlayerReportedPlayerEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "reason", ev.Reason }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      variables.AddPlayerVariables(ev.Target, "target");
      SCPDiscord.SendMessage("messages.onplayerreport", variables);
    }
  }
}