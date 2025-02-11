using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;

namespace SCPDiscord.EventListeners
{
  internal class PlayerEventListener : CustomEventsHandler
  {
    private readonly SCPDiscord plugin;

    // First dimension is target player second dimension is attacking player
    private static readonly Dictionary<Team, Team> teamKillingMatrix = new Dictionary<Team, Team>
    {
      { Team.FoundationForces, Team.Scientists       },
      { Team.ChaosInsurgency,  Team.ClassD           },
      { Team.Scientists,       Team.FoundationForces },
      { Team.ClassD,           Team.ChaosInsurgency  }
    };

    public PlayerEventListener(SCPDiscord pl)
    {
      plugin = pl;
    }

    private bool IsTeamDamage(Team attackerTeam, Team targetTeam)
    {
      if (!plugin.roundStarted)
      {
        return false;
      }

      if (attackerTeam == targetTeam)
      {
        return true;
      }

      foreach (KeyValuePair<Team, Team> team in teamKillingMatrix)
      {
        if (attackerTeam == team.Value && targetTeam == team.Key)
        {
          return true;
        }
      }

      return false;
    }

    // Convert damage handler to smod style damage type
    private string GetDamageType(DamageHandlerBase handler)
    {
      switch (handler)
      {
        case DisruptorDamageHandler _:
          return "disruptor";

        case ExplosionDamageHandler _:
          return "an explosion";

        case FirearmDamageHandler firearmDamageHandler:
          return firearmDamageHandler.WeaponType.ToString();

        case JailbirdDamageHandler _:
          return "a jailbird";

        case MicroHidDamageHandler _:
          return "a Micro-HID";

        case RecontainmentDamageHandler _:
          return "recontainment";

        case Scp018DamageHandler _:
          return "SCP-018";

        case Scp049DamageHandler _:
          return "SCP-049";

        case Scp096DamageHandler _:
          return "SCP-096";

        case ScpDamageHandler _:
          return "SCP attack";

        case Scp3114DamageHandler _:
          return "SCP-3114";

        case Scp939DamageHandler _:
          return "SCP-939";

        //case AttackerDamageHandler attackerDamageHandler:
        //	break;

        case CustomReasonDamageHandler _:
          return "UNKNOWN";

        case UniversalDamageHandler _:
          return "UNKNOWN";

        case WarheadDamageHandler _:
          return "alpha warhead";

        //case StandardDamageHandler standardDamageHandler:
        //	break;

        default:
          return "UNKNOWN";
      }
    }


    public override void OnPlayerHurt(PlayerHurtEventArgs ev)
    {
      if (ev.Target == null || ev.Target.Role == RoleTypeId.None || !(ev.DamageHandler is StandardDamageHandler stdHandler))
      {
        return;
      }

      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "damage",     stdHandler.Damage.ToString("0.##") },
        { "damagetype", GetDamageType(ev.DamageHandler)    }
      };

      if (ev.Player == null || ev.Target.PlayerId == ev.Player.PlayerId)
      {
        variables.AddPlayerVariables(ev.Target, "target");

        SCPDiscord.SendMessage("messages.onplayerhurt.noattacker", variables);
      }
      else
      {
        variables.AddPlayerVariables(ev.Target, "target");
        variables.AddPlayerVariables(ev.Player, "attacker");

        if (IsTeamDamage(ev.Player.ReferenceHub.GetTeam(), ev.Target.ReferenceHub.GetTeam()))
        {
          SCPDiscord.SendMessage("messages.onplayerhurt.friendlyfire", variables);
          return;
        }

        SCPDiscord.SendMessage("messages.onplayerhurt.default", variables);
      }
    }

    public override void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
      if (ev.Player == null || ev.Player.Role == RoleTypeId.None || !(ev.DamageHandler is StandardDamageHandler))
      {
        return;
      }

      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "damagetype", GetDamageType(ev.DamageHandler) }
      };

      if (ev.Attacker == null || ev.Player.PlayerId == ev.Attacker.PlayerId)
      {
        variables.AddPlayerVariables(ev.Player, "target");
        SCPDiscord.SendMessage("messages.onplayerdie.nokiller", variables);
      }
      else
      {
        variables.AddPlayerVariables(ev.Attacker, "attacker");
        variables.AddPlayerVariables(ev.Player, "target");

        if (IsTeamDamage(ev.Attacker.ReferenceHub.GetTeam(), ev.Player.ReferenceHub.GetTeam()))
        {
          SCPDiscord.SendMessage("messages.onplayerdie.friendlyfire", variables);
        }
        else
        {
          SCPDiscord.SendMessage("messages.onplayerdie.default", variables);
        }
      }
    }

    public override void OnPlayerPickedUpAmmo(PlayerPickedUpAmmoEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "ammo", ev.AmmoType.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.onplayerpickupammo", variables);
    }

    public override void OnPlayerPickedUpArmor(PlayerPickedUpArmorEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "armor", ev.Item?.Type.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerpickuparmor", variables);
    }

    public override void OnPlayerPickedUpScp330(PlayerPickedUpScp330EventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>();
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerpickupscp330", variables);
    }

    public override void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "item", ev.Item.Type.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerpickupitem", variables);
    }

    public override void OnPlayerDroppedAmmo(PlayerDroppedAmmoEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "ammo",   ev.Type.ToString()   },
        { "amount", ev.Amount.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerdropammo", variables);
    }

    public override void OnPlayerDroppedItem(PlayerDroppedItemEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "item", ev.Pickup.Type.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerdropitem", variables);
    }

    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
      if (ev.Player.PlayerId == Player.Host?.PlayerId)
      {
        return;
      }

      Dictionary<string, string> variables = new Dictionary<string, string>();
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerjoin", variables);
    }

    public override void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
      if (ev.Player?.PlayerId == Player.Host?.PlayerId || ev.Player?.UserId == null)
      {
        return;
      }

      Dictionary<string, string> variables = new Dictionary<string, string>();
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerleave", variables);
    }

    public override void OnPlayerSpawned(PlayerSpawnedEventArgs ev)
    {
      if (ev.Player?.UserId == null
          || ev.Player.UserId == Player.Host?.UserId
          || ev.Role.RoleTypeId == RoleTypeId.None
          || ev.Role.RoleTypeId == RoleTypeId.Spectator
          || ev.Role.RoleTypeId == RoleTypeId.Overwatch)
      {
        return;
      }

      Dictionary<string, string> variables = new Dictionary<string, string>();
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onspawn", variables);
    }

    public override void OnServerWaveRespawned(WaveRespawnedEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "players", ev.Players.Select(x => x.Nickname).ToString() }
      };
      SCPDiscord.SendMessage(ev.Team == Team.ChaosInsurgency ? "messages.onteamrespawn.ci" : "messages.onteamrespawn.mtf", variables);
    }

    public override void OnPlayerThrewProjectile(PlayerThrewProjectileEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "type", ev.Item.ItemTypeId.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onthrowprojectile", variables);
    }

    public override void OnPlayerUsedItem(PlayerUsedItemEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "item", ev.Item.Type.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onitemuse", variables);
    }

    public override void OnPlayerCuffed(PlayerCuffedEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>();
      if (ev.Player == null || ev.Player.PlayerId == Player.Host?.PlayerId)
      {
        variables.AddPlayerVariables(ev.Target, "target");
        SCPDiscord.SendMessage("messages.onhandcuff.nootherplayer", variables);
      }
      else
      {
        variables.AddPlayerVariables(ev.Target, "target");
        variables.AddPlayerVariables(ev.Player, "disarmer");
        SCPDiscord.SendMessage("messages.onhandcuff.default", variables);
      }
    }

    public override void OnPlayerUncuffed(PlayerUncuffedEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>();
      if (ev.Player != null && ev.Player.PlayerId == Player.Host?.PlayerId)
      {
        variables.AddPlayerVariables(ev.Target, "target");
        variables.AddPlayerVariables(ev.Player, "disarmer");
        SCPDiscord.SendMessage("messages.onhandcuffremoved.default", variables);
      }
      else
      {
        variables.AddPlayerVariables(ev.Target, "target");
        SCPDiscord.SendMessage("messages.onhandcuffremoved.nootherplayer", variables);
      }
    }

    // TODO: Check variables
    /*
    public override void OnPlayerReloadedWeapon(PlayerReloadedWeaponEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "weapon",      ev.Weapon.Type.ToString()                },
        { "maxclipsize", ev.Weapon.GetTotalMaxAmmo().ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onreload", variables);
    }
    */

    public override void OnServerGrenadeExploded(GrenadeExplodedEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "type", ev?.Grenade.Info.ItemId.ToString() }
      };

      if (ev?.Player != null)
      {
        variables.AddPlayerVariables(ev.Player, "player");
      }

      SCPDiscord.SendMessage("messages.ongrenadeexplosion", variables);
    }

    public override void OnPlayerEscaped(PlayerEscapedEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "newrole", ev.NewRole.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerescape", variables);
    }

    public override void OnPlayerReceivedEffect(PlayerReceivedEffectEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "effect", ev.Effect.ToString() },
        { "duration", ev.Duration.ToString() },
        { "intensity", ev.Intensity.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerreceiveeffect", variables);
    }
  }
}