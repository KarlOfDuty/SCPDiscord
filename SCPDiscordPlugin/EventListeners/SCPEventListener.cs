using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace SCPDiscord.EventListeners
{
	public class SCPEventListener
	{
		private readonly SCPDiscord plugin;
		public SCPEventListener(SCPDiscord pl)
		{
			plugin = pl;
		}

		[PluginEvent(ServerEventType.Scp079LockDoor)]
		public void On079Lock(Player player, DoorVariant door)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "door",          door.name                                },
				//{ "open",          ev.Door.IsOpen.ToString()                },
				{ "ipaddress",     player.IpAddress                         },
				{ "name",          player.Nickname                          },
				{ "playerid",      player.PlayerId.ToString()               },
				{ "steamid",       player.GetParsedUserID()                 },
				{ "class",         player.Role.ToString()                   },
				{ "team",          player.ReferenceHub.GetTeam().ToString() }
			};

			plugin.SendMessage("messages.on079lockdoor", variables);
		}

		/*
		public void On079Elevator()
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "apdrain",                    ev.APDrain.ToString()                },
				{ "elevator",                   ev.Elevator.ElevatorType.ToString()  },
				{ "status",                     ev.Elevator.ElevatorStatus.ToString()},
				{ "ipaddress",                  ev.Player.IPAddress                  },
				{ "name",                       ev.Player.Name                       },
				{ "playerid",                   ev.Player.PlayerID.ToString()        },
				{ "steamid",                    ev.Player.GetParsedUserID() ?? ev.Player.UserID },
				{ "class",                      ev.Player.Role.ToString()     },
				{ "team",                       ev.Player.ReferenceHub.GetTeam().ToString()   }
			};
			if (ev.Elevator.ElevatorStatus == ElevatorStatus.DOWN)
			{
				plugin.SendMessage(Config.GetArray("messages.on079elevator.up"), "messages.on079elevator.up", variables);
			}
			else if (ev.Elevator.ElevatorStatus == ElevatorStatus.UP)
			{
				plugin.SendMessage(Config.GetArray("messages.on079elevator.down"), "messages.on079elevator.down", variables);
			}
		}
		*/

		/*
		public void OnPlayerInfected(PlayerInfectedEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "damage",                 ev.Damage.ToString()                    },
				{ "infecttime",             ev.InfectTime.ToString()                },
				{ "attackeripaddress",      ev.Attacker.IPAddress                   },
				{ "attackername",           ev.Attacker.Name                        },
				{ "attackerplayerid",       ev.Attacker.PlayerID.ToString()         },
				{ "attackersteamid",        ev.Attacker.GetParsedUserID() ?? ev.Player.UserID },
				{ "attackerclass",          ev.Attacker.Role.ToString()      },
				{ "attackerteam",           ev.Attacker.ReferenceHub.GetTeam().ToString()    },
				{ "playeripaddress",        ev.Attacker.IPAddress                   },
				{ "playername",             ev.Player.Name                          },
				{ "playerplayerid",         ev.Player.PlayerID.ToString()           },
				{ "playersteamid",          ev.Player.GetParsedUserID() ?? ev.Player.UserID },
				{ "playerclass",            ev.Player.Role.ToString()        },
				{ "playerteam",             ev.Player.ReferenceHub.GetTeam().ToString()      }
			};
			plugin.SendMessage(Config.GetArray("messages.onplayerinfected"), "messages.onplayerinfected", variables);
		}
		*/

		[PluginEvent(ServerEventType.Scp079UseTesla)]
		public void On079TeslaGate(Player player, TeslaGate teslaGate)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress",     player.IpAddress                         },
				{ "name",          player.Nickname                          },
				{ "playerid",      player.PlayerId.ToString()               },
				{ "steamid",       player.GetParsedUserID()                 },
				{ "class",         player.Role.ToString()                   },
				{ "team",          player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.on079teslagate", variables);
		}

		[PluginEvent(ServerEventType.Scp079GainExperience)]
		public void On079AddExp(Player player, int amount, Scp079HudTranslation reason)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "xptype",        reason.ToString()                        },
				{ "amount",        amount.ToString()                        },
				{ "ipaddress",     player.IpAddress                         },
				{ "name",          player.Nickname                          },
				{ "playerid",      player.PlayerId.ToString()               },
				{ "steamid",       player.GetParsedUserID()                 },
				{ "class",         player.Role.ToString()                   },
				{ "team",          player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.on079addexp", variables);
		}

		[PluginEvent(ServerEventType.Scp079LevelUpTier)]
		public void On079LevelUp(Player player, int level)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "level",         level.ToString()                         },
				{ "ipaddress",     player.IpAddress                         },
				{ "name",          player.Nickname                          },
				{ "playerid",      player.PlayerId.ToString()               },
				{ "steamid",       player.GetParsedUserID()                 },
				{ "class",         player.Role.ToString()                   },
				{ "team",          player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.on079levelup", variables);
		}

		[PluginEvent(ServerEventType.Scp079UnlockDoor)]
		public void On079UnlockDoor(Player player, DoorVariant door)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "doorname",      door.name                                },
				{ "ipaddress",     player.IpAddress                         },
				{ "name",          player.Nickname                          },
				{ "playerid",      player.PlayerId.ToString()               },
				{ "steamid",       player.GetParsedUserID()                 },
				{ "class",         player.Role.ToString()                   },
				{ "team",          player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.on079unlockdoor", variables);
		}

		[PluginEvent(ServerEventType.Scp079LockdownRoom)]
		public void On079Lockdown(Player player, RoomIdentifier room)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "room",          room.name                                },
				{ "ipaddress",     player.IpAddress                         },
				{ "name",          player.Nickname                          },
				{ "playerid",      player.PlayerId.ToString()               },
				{ "steamid",       player.GetParsedUserID()                 },
				{ "class",         player.Role.ToString()                   },
				{ "team",          player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.on079lockdown", variables);
		}

		[PluginEvent(ServerEventType.Scp079CancelRoomLockdown)]
		public void On079CancelLockdown(Player player, RoomIdentifier room)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "room",          room.name                                },
				{ "ipaddress",     player.IpAddress                         },
				{ "name",          player.Nickname                          },
				{ "playerid",      player.PlayerId.ToString()               },
				{ "steamid",       player.GetParsedUserID()                 },
				{ "class",         player.Role.ToString()                   },
				{ "team",          player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.on079cancellockdown", variables);
		}

		[PluginEvent(ServerEventType.Scp049ResurrectBody)]
		public void OnRecallZombie(Player player, Player target, BasicRagdoll ragdoll)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "targetipaddress",    target.IpAddress                         },
				{ "targetname",         target.Nickname                          },
				{ "targetplayerid",     target.PlayerId.ToString()               },
				{ "targetsteamid",      target.GetParsedUserID()                 },
				{ "targetclass",        target.Role.ToString()                   },
				{ "targetteam",         target.ReferenceHub.GetTeam().ToString() },
				{ "playeripaddress",    player.IpAddress                         },
				{ "playername",         player.Nickname                          },
				{ "playerplayerid",     player.PlayerId.ToString()               },
				{ "playersteamid",      player.GetParsedUserID()                 },
				{ "playerclass",        player.Role.ToString()                   },
				{ "playerteam",         player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.onrecallzombie", variables);
		}
	}
}