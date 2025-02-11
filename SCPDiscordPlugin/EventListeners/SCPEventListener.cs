using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.CustomHandlers;

namespace SCPDiscord.EventListeners
{
  public class SCPEventListener : CustomEventsHandler
  {
    private readonly SCPDiscord plugin;

    public SCPEventListener(SCPDiscord pl)
    {
      plugin = pl;
    }

    // TODO: Check the information in these events
    public override void OnScp079LockedDoor(Scp079LockedDoorEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "door", ev.Door.Base.DoorName }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.on079lockdoor", variables);
    }

    public override void OnScp079UsedTesla(Scp079UsedTeslaEventArgs ev)
    {
      Dictionary<string, string> variables = new();
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.on079teslagate", variables);
    }

    public override void OnScp079LeveledUp(Scp079LeveledUpEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "level", ev.Tier.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.on079levelup", variables);
    }

    public override void OnScp079UnlockedDoor(Scp079UnlockedDoorEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "doorname", ev.Door.Base.name }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.on079unlockdoor", variables);
    }

    public override void OnScp079LockedDownRoom(Scp079LockedDownRoomEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "room", ev.Room.Base.name }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.on079lockdown", variables);
    }

    public override void OnScp079CancelledRoomLockdown(Scp079CancelledRoomLockdownEventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "room", ev.Room.Base.name }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.on079cancellockdown", variables);
    }

    public override void OnScp049ResurrectedBody(Scp049ResurrectedBodyEventArgs ev)
    {
      Dictionary<string, string> variables = new();
      variables.AddPlayerVariables(ev.Target, "target");
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.onrecallzombie", variables);
    }

    public override void OnPlayerInteractedScp330(PlayerInteractedScp330EventArgs ev)
    {
      Dictionary<string, string> variables = new()
      {
        { "uses", ev.Uses.ToString() }
      };
      variables.AddPlayerVariables(ev.Player, "player");

      SCPDiscord.SendMessage("messages.oninteract330", variables);
    }
  }
}