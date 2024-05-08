using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Ragdolls;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace SCPDiscord.EventListeners
{
	public class SCPEventListener
	{
		private readonly SCPDiscord plugin;
		public SCPEventListener(SCPDiscord pl)
		{
			plugin = pl;
		}

		[PluginEvent]
		public void On079Lock(Scp079LockDoorEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "door", ev.Door.name }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.on079lockdoor", variables);
		}

		[PluginEvent]
		public void On079TeslaGate(Scp079UseTeslaEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.on079teslagate", variables);
		}

		[PluginEvent]
		public void On079AddExp(Scp079GainExperienceEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "xptype", ev.Reason.ToString() },
				{ "amount", ev.Amount.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.on079addexp", variables);
		}

		[PluginEvent]
		public void On079LevelUp(Scp079LevelUpTierEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "level", ev.Tier.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.on079levelup", variables);
		}

		[PluginEvent]
		public void On079UnlockDoor(Scp079UnlockDoorEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "doorname", ev.Door.name }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.on079unlockdoor", variables);
		}

		[PluginEvent]
		public void On079Lockdown(Scp079LockdownRoomEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "room", ev.Room.name }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.on079lockdown", variables);
		}

		[PluginEvent]
		public void On079CancelLockdown(Scp079CancelRoomLockdownEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "room", ev.Room.name }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.on079cancellockdown", variables);
		}

		[PluginEvent]
		public void OnRecallZombie(Scp049ResurrectBodyEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Target, "target");
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.onrecallzombie", variables);
		}

        public void OnInteractScp330(PlayerInteractScp330Event ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "uses", ev.Uses.ToString() }
			};
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.oninteract330", variables);
        }
    }
}