using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MapGeneration;

namespace SCPDiscord.EventListeners
{
  internal class EnvironmentEventListener : CustomEventsHandler
  {
    private readonly SCPDiscord plugin;

    public EnvironmentEventListener(SCPDiscord pl)
    {
      plugin = pl;
    }

    public override void OnPlayerLeftPocketDimension(PlayerLeftPocketDimensionEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "successful", ev.IsSuccessful.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onpocketdimensionexit", variables);
    }

    public override void OnPlayerEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs ev)
    {
      Dictionary<string, string> variables = new();
      variables.AddPlayerVariables(ev.Player, "target");
      //variables.AddPlayerVariables(ev.Attacker, "attacker"); // TODO: Fix
      SCPDiscord.SendMessage("messages.onpocketdimensionenter", variables);
    }

    public override void OnScp914Activated(Scp914ActivatedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "knobsetting", ev.KnobSetting.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onscp914activate", variables);
    }

    public override void OnPlayerInteractedElevator(PlayerInteractedElevatorEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "elevatorname", ev.Elevator.Group.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.onelevatoruse", variables);
    }

    // TODO: Can I still check if it is resumed
    public override void OnWarheadStarted(WarheadStartedEventArgs ev)
    {
      Dictionary<string, string> variables = new Dictionary<string, string>
      {
        { "isAutomatic", ev.IsAutomatic.ToString()            },
        { "timeleft",    Warhead.DetonationTime.ToString("0") }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      // TODO: Add deadmans switch message
      if (ev.Player == null || ev.Player.PlayerId == Player.Host?.PlayerId)
      {
        SCPDiscord.SendMessage(ev.WarheadState.ScenarioType == WarheadScenarioType.Resume ? "messages.onstartcountdown.server.resumed" : "messages.onstartcountdown.server.initiated", variables);
      }
      else
      {
        SCPDiscord.SendMessage(ev.WarheadState.ScenarioType == WarheadScenarioType.Resume ? "messages.onstartcountdown.player.resumed" : "messages.onstartcountdown.player.initiated", variables);
      }
    }

    public override void OnWarheadStopped(WarheadStoppedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "timeleft", Warhead.DetonationTime.ToString("0.##") }
      };

      if (ev.Player == null || ev.Player.PlayerId == Player.Host?.PlayerId)
      {
        SCPDiscord.SendMessage("messages.onstopcountdown.noplayer", variables);
      }
      else
      {
        variables.AddPlayerVariables(ev.Player, "player");
        SCPDiscord.SendMessage("messages.onstopcountdown.default", variables);
      }
    }

    public override void OnWarheadDetonated(WarheadDetonatedEventArgs ev)
    {
      SCPDiscord.SendMessage("messages.ondetonate");
    }

    public override void OnServerLczDecontaminationStarted()
    {
      SCPDiscord.SendMessage("messages.ondecontaminate");
    }

    public override void OnServerGeneratorActivated(GeneratorActivatedEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "room", ev.Generator.Room?.Name.ToString() },
      };
      SCPDiscord.SendMessage("messages.ongeneratorfinish", variables);
    }

    // TODO: Doesn't seem to exist anymore
    /*public override void OnPlayerUnlockGenerator(PlayerUnlockGeneratorEvent ev)
    {
          if (ev.Player == null) return;
          Dictionary<string, string> variables = new Dictionary<string, string>
          {
            { "engaged",    ev.Generator?.Engaged.ToString() },
            { "activating", ev.Generator?.Activating.ToString() },
            { "room",       ev.Generator?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
          };
          variables.AddPlayerVariables(ev.Player, "player");
          SCPDiscord.SendMessage("messages.ongeneratorunlock", variables);
    }*/

    public override void OnPlayerOpenedGenerator(PlayerOpenedGeneratorEventArgs ev)
    {
      if (ev.Player == null) return;
      Dictionary<string, string> variables = new()
      {
        { "engaged",    ev.Generator?.Engaged.ToString() },
        { "activating", ev.Generator?.Activating.ToString() },
        { "room",       ev.Generator?.Room?.Name.ToString() },
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.ongeneratoropen", variables);
    }

    public override void OnPlayerClosedGenerator(PlayerClosedGeneratorEventArgs ev)
    {
      if (ev.Player == null) return;

      Dictionary<string, string> variables = new()
      {
        { "engaged",    ev.Generator?.Engaged.ToString() },
        { "activating", ev.Generator?.Activating.ToString() },
        { "room",       ev.Generator?.Room?.Name.ToString() },
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.ongeneratorclose", variables);
    }

    public override void OnPlayerActivatedGenerator(PlayerActivatedGeneratorEventArgs ev)
    {
      if (ev.Player == null) return;

      Dictionary<string, string> variables = new()
      {
        { "engaged",    ev.Generator?.Engaged.ToString() },
        { "activating", ev.Generator?.Activating.ToString() },
        { "room",       ev.Generator?.Room?.Name.ToString() },
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.ongeneratoractivated", variables);
    }

    public override void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEventArgs ev)
    {
      if (ev.Player == null) return;

      Dictionary<string, string> variables = new()
      {
        { "engaged",    ev.Generator?.Engaged.ToString() },
        { "activating", ev.Generator?.Activating.ToString() },
        { "room",       ev.Generator?.Room?.Name.ToString() },
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.ongeneratordeactivated", variables);
    }

    public override void OnServerMapGenerated(MapGeneratedEventArgs ev)
    {
      SCPDiscord.SendMessage("messages.onmapgenerated");
    }

    public override void OnPlayerInteractedLocker(PlayerInteractedLockerEventArgs ev)
    {
      if (ev.Player == null) return;

      Dictionary<string, string> variables = new()
      {
          { "chamber", ev.Chamber?.ToString() },
          { "room",    ev.Chamber?.Base.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
          { "canopen", ev.CanOpen.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");
      SCPDiscord.SendMessage("messages.onplayerinteractlocker", variables);
    }
  }
}
