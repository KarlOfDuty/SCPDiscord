﻿using System.Collections.Generic;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MapGeneration.Distributors;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Scp914;

namespace SCPDiscord.EventListeners
{
	internal class EnvironmentEventListener
	{
		private readonly SCPDiscord plugin;

		public EnvironmentEventListener(SCPDiscord pl)
		{
			plugin = pl;
		}

		[PluginEvent]
		public void OnDoorAccess(PlayerInteractDoorEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "doorname",   ev.Door.name },
				{ "permission", ev.Door.RequiredPermissions.RequiredPermissions.ToString() },
				{ "open",       ev.Door.TargetState.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage(ev.CanOpen ? "messages.ondooraccess.allowed" : "messages.ondooraccess.denied", variables);
		}

		[PluginEvent]
		public void OnPocketDimensionExit(PlayerExitPocketDimensionEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "successful", ev.IsSuccessful.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onpocketdimensionexit", variables);
		}

		[PluginEvent]
		public void OnPocketDimensionEnter(Scp106TeleportPlayerEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Target, "target");
			variables.AddPlayerVariables(ev.Player, "attacker");
			plugin.SendMessage("messages.onpocketdimensionenter", variables);
		}

		[PluginEvent]
		public void OnSCP914Activate(Scp914ActivateEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "knobsetting", ev.KnobSetting.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onscp914activate", variables);
		}

		[PluginEvent]
		public void OnSCP914ChangeKnob(Scp914KnobChangeEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "newsetting", ev.KnobSetting.ToString()         },
				{ "oldsetting", ev.PreviousKnobSetting.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onscp914changeknob", variables);
		}

		[PluginEvent]
		public void OnElevatorUse(PlayerInteractElevatorEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "elevatorname", ev.Elevator.AssignedGroup.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.onelevatoruse", variables);
		}

		[PluginEvent]
		public void OnStartCountdown(WarheadStartEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "isAutomatic", ev.IsAutomatic.ToString()            },
				{ "timeleft",    Warhead.DetonationTime.ToString("0") }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			if (ev.Player == null || ev.Player.PlayerId == Server.Instance.PlayerId)
			{
				plugin.SendMessage(ev.IsResumed ? "messages.onstartcountdown.server.resumed" : "messages.onstartcountdown.server.initiated", variables);
			}
			else
			{
				plugin.SendMessage(ev.IsResumed ? "messages.onstartcountdown.player.resumed" : "messages.onstartcountdown.player.initiated", variables);
			}
		}

		[PluginEvent]
		public void OnStopCountdown(WarheadStopEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "timeleft", Warhead.DetonationTime.ToString("0.##") }
			};

			if (ev.Player == null || ev.Player.PlayerId == Server.Instance.PlayerId)
			{
				plugin.SendMessage("messages.onstopcountdown.noplayer", variables);
			}
			else
			{
				variables.AddPlayerVariables(ev.Player, "player");
				plugin.SendMessage("messages.onstopcountdown.default", variables);
			}
		}

		[PluginEvent]
		public void OnDetonate(WarheadDetonationEvent ev)
		{
			plugin.SendMessage("messages.ondetonate");
		}

		[PluginEvent]
		public void OnDecontaminate(LczDecontaminationStartEvent ev)
		{
			plugin.SendMessage("messages.ondecontaminate");
		}

		[PluginEvent]
		public void OnGeneratorFinish(GeneratorActivatedEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "room", ev.Generator?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
			};
			plugin.SendMessage("messages.ongeneratorfinish", variables);
		}

		[PluginEvent]
        public void OnPlayerUnlockGenerator(PlayerUnlockGeneratorEvent ev)
        {
	        if (ev.Player == null) return;
        	Dictionary<string, string> variables = new Dictionary<string, string>
        	{
        		{ "engaged",    ev.Generator?.Engaged.ToString() },
        		{ "activating", ev.Generator?.Activating.ToString() },
        		{ "room",       ev.Generator?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
        	};
	        variables.AddPlayerVariables(ev.Player, "player");
        	plugin.SendMessage("messages.ongeneratorunlock", variables);
        }

        [PluginEvent]
        public void OnPlayerOpenGenerator(PlayerOpenGeneratorEvent ev)
        {
	        if (ev.Player == null) return;
        	Dictionary<string, string> variables = new Dictionary<string, string>
        	{
        		{ "engaged",    ev.Generator?.Engaged.ToString() },
        		{ "activating", ev.Generator?.Activating.ToString() },
        		{ "room",       ev.Generator?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
        	};
	        variables.AddPlayerVariables(ev.Player, "player");
        	plugin.SendMessage("messages.ongeneratoropen", variables);
        }

        [PluginEvent]
        public void OnPlayerCloseGenerator(PlayerCloseGeneratorEvent ev)
        {
	        if (ev.Player == null) return;

        	Dictionary<string, string> variables = new Dictionary<string, string>
        	{
        		{ "engaged",    ev.Generator?.Engaged.ToString() },
        		{ "activating", ev.Generator?.Activating.ToString() },
        		{ "room",       ev.Generator?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
        	};
	        variables.AddPlayerVariables(ev.Player, "player");
        	plugin.SendMessage("messages.ongeneratorclose", variables);
        }

        [PluginEvent]
        public void OnPlayerActivateGenerator(PlayerActivateGeneratorEvent ev)
        {
	        if (ev.Player == null) return;

        	Dictionary<string, string> variables = new Dictionary<string, string>
        	{
        		{ "engaged",    ev.Generator?.Engaged.ToString() },
        		{ "activating", ev.Generator?.Activating.ToString() },
        		{ "room",       ev.Generator?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
        	};
	        variables.AddPlayerVariables(ev.Player, "player");
        	plugin.SendMessage("messages.ongeneratoractivated", variables);
        }

        [PluginEvent]
        public void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEvent ev)
        {
	        if (ev.Player == null) return;

        	Dictionary<string, string> variables = new Dictionary<string, string>
        	{
        		{ "engaged",    ev.Generator?.Engaged.ToString() },
        		{ "activating", ev.Generator?.Activating.ToString() },
        		{ "room",       ev.Generator?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
        	};
			variables.AddPlayerVariables(ev.Player, "player");
        	plugin.SendMessage("messages.ongeneratordeactivated", variables);
        }

		[PluginEvent]
        public void OnMapGenerated(MapGeneratedEvent ev)
		{
			plugin.SendMessage("messages.onmapgenerated");
		}

		[PluginEvent]
        public void OnItemSpawned(ItemSpawnedEvent ev)
		{
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "item", ev.Item.ToString() }
            };
            plugin.SendMessage("messages.onitemspawned", variables);
        }

		[PluginEvent]
        public void OnPlaceBlood(PlaceBloodEvent ev)
        {
	        if (ev.Player == null) return;

            Dictionary<string, string> variables = new Dictionary<string, string> { };
            variables.AddPlayerVariables(ev.Player, "player");
            plugin.SendMessage("messages.onplaceblood", variables);
        }

		[PluginEvent]
        public void OnPlayerInteractLocker(PlayerInteractLockerEvent ev)
        {
	        if (ev.Player == null) return;

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "chamber", ev.Chamber?.ToString() },
                { "room",    ev.Chamber?.GetComponentInParent<RoomIdentifier>()?.Name.ToString() },
				{ "canopen", ev.CanOpen.ToString() }
            };
            variables.AddPlayerVariables(ev.Player, "player");
            plugin.SendMessage("messages.onplayerinteractlocker", variables);
        }

		[PluginEvent]
        public void OnPlayerInteractShootingTarget(PlayerInteractShootingTargetEvent ev)
        {
	        if (ev.Player == null) return;

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "target", ev.ShootingTarget.ToString() },
            };
            variables.AddPlayerVariables(ev.Player, "player");
            plugin.SendMessage("messages.onplayerinteractshootingtarget", variables);
        }
    }
}