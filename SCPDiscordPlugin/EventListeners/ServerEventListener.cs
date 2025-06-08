using System;
using System.Collections.Generic;
using GameCore;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RemoteAdmin;

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

    public override void OnServerBanRevoked(BanRevokedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "name",     ev.BanDetails.OriginalName },
        { "issuer",   ev.BanDetails.Issuer },
        { "reason",   ev.BanDetails.Reason },
        { "duration", Utilities.TicksToCompoundTime(Math.Min(ev.BanDetails.Expires - ev.BanDetails.IssuanceTime, 0)) }
      };

      if (ev.BanType == BanHandler.BanType.IP)
      {
        variables.Add("ip", ev.BanDetails.Id);
        SCPDiscord.SendMessage("messages.onbanrevoked.ip", variables);
      }
      else if(Utilities.IsPossibleSteamID(ev.BanDetails.Id, out ulong id))
      {
        variables.Add("userid", id.ToString());
        SCPDiscord.SendMessage("messages.onbanrevoked.userid", variables);
      }
      else
      {
        variables.Add("userid", ev.BanDetails.Id);
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

    public override void OnServerCommandExecuted(CommandExecutedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "command",       (ev.Command.Command + " " + string.Join(" ", ev.Arguments)).Trim() },
        { "result",        ev.ExecutedSuccessfully.ToString() },
        { "returnmessage", ev.Response }
      };

      string senderType = "server";
      if (ev.Sender is PlayerCommandSender playerSender && Player.Get(playerSender.ReferenceHub) != null)
      {
        variables.AddPlayerVariables(Player.Get(playerSender.ReferenceHub), "player");
        senderType = "player";
      }

      switch (ev.CommandType)
      {
        case CommandType.Console:
          SCPDiscord.SendMessage($"messages.onexecutedcommand.console.{senderType}", variables);
          break;
        case CommandType.RemoteAdmin:
          SCPDiscord.SendMessage($"messages.onexecutedcommand.remoteadmin.{senderType}", variables);
          break;
        case CommandType.Client:
          SCPDiscord.SendMessage($"messages.onexecutedcommand.client.{senderType}", variables);
          break;
        default:
          Logger.Error($"OnServerCommandExecuted: Unknown command type: {ev.CommandType}");
          break;
      }
    }

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

    public override void OnServerRoundEnded(RoundEndedEventArgs ev)
    {
      if (plugin.roundStarted && RoundStart.RoundLength.TotalSeconds > 60)
      {
        Dictionary<string, string> variables = new()
        {
          { "duration",         (RoundStart.RoundLength.TotalSeconds / 60).ToString("0") },
          { "leadingteam",       ev.LeadingTeam.ToString() },
          { "dclassalive",       Utilities.GetNumberOfPlayers(Team.ClassD).ToString() },
          { "dclassdead",        "-" }, //Statistics.CurrentRound.ClassDDead.ToString() },
          { "dclassescaped",     RoundSummary.EscapedClassD.ToString() },
          { "dclassstart",       RoundSummary.singleton.classlistStart.class_ds.ToString() },
          { "mtfalive",          Utilities.GetNumberOfPlayers(Team.FoundationForces).ToString() },
          { "mtfdead",           "-" }, //Statistics.CurrentRound.MtfAndGuardsDead.ToString()  },
          { "mtfstart",          RoundSummary.singleton.classlistStart.mtf_and_guards.ToString() },
          { "scientistsalive",   Utilities.GetNumberOfPlayers(Team.Scientists).ToString()   },
          { "scientistsdead",    "-" }, //Statistics.CurrentRound.ScientistsDead.ToString() },
          { "scientistsescaped", RoundSummary.EscapedScientists.ToString() },
          { "scientistsstart",   RoundSummary.singleton.classlistStart.scientists.ToString()   },
          { "scpalive",          RoundSummary.SurvivingSCPs.ToString() },
          { "scpdead",           "-" }, //Statistics.CurrentRound.ScpsDead.ToString() },
          { "scpkills",          RoundSummary.KilledBySCPs.ToString() },
          { "scpstart",          RoundSummary.singleton.classlistStart.scps_except_zombies.ToString() },
          { "warheaddetonated",  Warhead.IsDetonated.ToString() },
          { "warheadkills",      AlphaWarheadController.Singleton.WarheadKills.ToString() },
          { "zombiesalive",      Utilities.GetNumberOfPlayers(RoleTypeId.Scp0492).ToString() },
          { "zombieschanged",    RoundSummary.ChangedIntoZombies.ToString() }
        };
        SCPDiscord.SendMessage("messages.onroundend", variables);
      }

      plugin.roundStarted = false;
    }

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