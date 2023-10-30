using System.Collections.Generic;
using System.Linq;
using Footprinting;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Radio;
using InventorySystem.Items.ThrowableProjectiles;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Respawning;
using UnityEngine;

namespace SCPDiscord.EventListeners
{
	internal class PlayerEventListener
	{
		private readonly SCPDiscord plugin;

		// First dimension is target player second dimension is attacking player
		private static readonly Dictionary<Team, Team> teamKillingMatrix = new Dictionary<Team, Team>
		{
			{ Team.FoundationForces, Team.Scientists },
			{ Team.ChaosInsurgency, Team.ClassD },
			{ Team.Scientists, Team.FoundationForces },
			{ Team.ClassD, Team.ChaosInsurgency }
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

				case ExplosionDamageHandler explosionDamageHandler:
					return "an explosion";

				case FirearmDamageHandler firearmDamageHandler:
					return firearmDamageHandler.WeaponType.ToString();

				case MicroHidDamageHandler microHidDamageHandler:
					return "Micro-HID";

				case RecontainmentDamageHandler recontainmentDamageHandler:
					return "recontainment";

				case Scp018DamageHandler scp018DamageHandler:
					return "SCP-018";

				case Scp049DamageHandler scp049DamageHandler:
					return "SCP-049";

				case Scp096DamageHandler scp096DamageHandler:
					return "SCP-096";

				case ScpDamageHandler scpDamageHandler:
					return "SCP attack";

				case Scp939DamageHandler scp939DamageHandler:
					return "SCP-939";

				//case AttackerDamageHandler attackerDamageHandler:
				//	break;

				case CustomReasonDamageHandler customReasonDamageHandler:
					return "UNKNOWN";

				case UniversalDamageHandler universalDamageHandler:
					return "UNKNOWN";

				case WarheadDamageHandler warheadDamageHandler:
					return "alpha warhead";

				//case StandardDamageHandler standardDamageHandler:
				//	break;

				default:
					return "UNKNOWN";
			}
		}

		[PluginEvent(ServerEventType.PlayerDamage)]
		public void OnPlayerHurt(PlayerDamageEvent ev)
		{
			if (ev.Target == null || ev.Target.Role == RoleTypeId.None || !(ev.DamageHandler is StandardDamageHandler stdHandler))
			{
				return;
			}

			if (ev.Player == null || ev.Target.PlayerId == ev.Player.PlayerId)
			{
				Dictionary<string, string> noAttackerVar = new Dictionary<string, string>
				{
					{ "damage",             stdHandler.Damage.ToString("0.##")          },
					{ "damagetype",         GetDamageType(ev.DamageHandler)             },
					{ "playeripaddress",    ev.Target.IpAddress                         },
					{ "playername",         ev.Target.Nickname                          },
					{ "playerplayerid",     ev.Target.PlayerId.ToString()               },
					{ "playersteamid",      ev.Target.GetParsedUserID()                 },
					{ "playerclass",        ev.Target.Role.ToString()                   },
					{ "playerteam",         ev.Target.ReferenceHub.GetTeam().ToString() }
				};
				plugin.SendMessage("messages.onplayerhurt.noattacker", noAttackerVar);
				return;
			}

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "damage",             stdHandler.Damage.ToString("0.##")          },
				{ "damagetype",         GetDamageType(ev.DamageHandler)             },
				{ "attackeripaddress",  ev.Player.IpAddress                         },
				{ "attackername",       ev.Player.Nickname                          },
				{ "attackerplayerid",   ev.Player.PlayerId.ToString()               },
				{ "attackersteamid",    ev.Player.GetParsedUserID()                 },
				{ "attackerclass",      ev.Player.Role.ToString()                   },
				{ "attackerteam",       ev.Player.ReferenceHub.GetTeam().ToString() },
				{ "playeripaddress",    ev.Target.IpAddress                         },
				{ "playername",         ev.Target.Nickname                          },
				{ "playerplayerid",     ev.Target.PlayerId.ToString()               },
				{ "playersteamid",      ev.Target.GetParsedUserID()                 },
				{ "playerclass",        ev.Target.Role.ToString()                   },
				{ "playerteam",         ev.Target.ReferenceHub.GetTeam().ToString() }
			};

			if (IsTeamDamage(ev.Player.ReferenceHub.GetTeam(), ev.Target.ReferenceHub.GetTeam()))
			{
				plugin.SendMessage("messages.onplayerhurt.friendlyfire", variables);
				return;
			}

			plugin.SendMessage("messages.onplayerhurt.default", variables);
		}

		[PluginEvent(ServerEventType.PlayerDying)]
		public void OnPlayerDie(PlayerDyingEvent ev)
		{
			if (ev.Player == null || ev.Player.Role == RoleTypeId.None || !(ev.DamageHandler is StandardDamageHandler stdHandler))
			{
				return;
			}

			if (ev.Attacker == null || ev.Player.PlayerId == ev.Attacker.PlayerId)
			{
				Dictionary<string, string> noKillerVar = new Dictionary<string, string>
				{
					{ "damagetype",         GetDamageType(ev.DamageHandler)             },
					{ "playeripaddress",    ev.Player.IpAddress                         },
					{ "playername",         ev.Player.Nickname                          },
					{ "playerplayerid",     ev.Player.PlayerId.ToString()               },
					{ "playersteamid",      ev.Player.GetParsedUserID()                 },
					{ "playerclass",        ev.Player.Role.ToString()                   },
					{ "playerteam",         ev.Player.ReferenceHub.GetTeam().ToString() }
				};
				plugin.SendMessage("messages.onplayerdie.nokiller", noKillerVar);
				return;
			}

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "damagetype",         GetDamageType(ev.DamageHandler)               },
				{ "attackeripaddress",  ev.Attacker.IpAddress                         },
				{ "attackername",       ev.Attacker.Nickname                          },
				{ "attackerplayerid",   ev.Attacker.PlayerId.ToString()               },
				{ "attackersteamid",    ev.Attacker.GetParsedUserID()                 },
				{ "attackerclass",      ev.Attacker.Role.ToString()                   },
				{ "attackerteam",       ev.Attacker.ReferenceHub.GetTeam().ToString() },
				{ "playeripaddress",    ev.Player.IpAddress                           },
				{ "playername",         ev.Player.Nickname                            },
				{ "playerplayerid",     ev.Player.PlayerId.ToString()                 },
				{ "playersteamid",      ev.Player.GetParsedUserID()                   },
				{ "playerclass",        ev.Player.Role.ToString()                     },
				{ "playerteam",         ev.Player.ReferenceHub.GetTeam().ToString()   }
			};

			if (IsTeamDamage(ev.Attacker.ReferenceHub.GetTeam(), ev.Player.ReferenceHub.GetTeam()))
			{
				plugin.SendMessage("messages.onplayerdie.friendlyfire", variables);
				return;
			}
			plugin.SendMessage("messages.onplayerdie.default", variables);
		}

		[PluginEvent(ServerEventType.PlayerPickupAmmo)]
		public void OnPlayerPickupAmmo(Player player, ItemPickupBase ammo)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ammo",         ammo.ToString()                    },
				{ "ipaddress",    player.IpAddress                   },
				{ "name",         player.Nickname                        },
				{ "playerid",     player.PlayerId.ToString()         },
				{ "steamid",      player.GetParsedUserID()                     },
				{ "class",        player.Role.ToString()    },
				{ "team",         player.ReferenceHub.GetTeam().ToString()    }
			};
			plugin.SendMessage("messages.onplayerpickupammo", variables);
		}

		[PluginEvent(ServerEventType.PlayerPickupArmor)]
		public void OnPlayerPickupArmor(Player player, ItemPickupBase armor)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "armor",         armor.ToString()                    },
				{ "ipaddress",    player.IpAddress                   },
				{ "name",         player.Nickname                        },
				{ "playerid",     player.PlayerId.ToString()         },
				{ "steamid",      player.GetParsedUserID()                     },
				{ "class",        player.Role.ToString()    },
				{ "team",         player.ReferenceHub.GetTeam().ToString()    }
			};
			plugin.SendMessage("messages.onplayerpickuparmor", variables);
		}

		[PluginEvent(ServerEventType.PlayerPickupScp330)]
		public void OnPlayerPickupSCP330(Player player, ItemPickupBase scp330)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress",    player.IpAddress                   },
				{ "name",         player.Nickname                        },
				{ "playerid",     player.PlayerId.ToString()         },
				{ "steamid",      player.GetParsedUserID()                     },
				{ "class",        player.Role.ToString()    },
				{ "team",         player.ReferenceHub.GetTeam().ToString()    }
			};
			plugin.SendMessage("messages.onplayerpickupscp330", variables);
		}

		[PluginEvent(ServerEventType.PlayerSearchedPickup)]
		public void OnPlayerPickupItem(Player player, ItemPickupBase item)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "item",         item.ToString()                    },
				{ "ipaddress",    player.IpAddress                   },
				{ "name",         player.Nickname                        },
				{ "playerid",     player.PlayerId.ToString()         },
				{ "steamid",      player.GetParsedUserID()                     },
				{ "class",        player.Role.ToString()    },
				{ "team",         player.ReferenceHub.GetTeam().ToString()    }
			};
			plugin.SendMessage("messages.onplayerpickupitem", variables);
		}

		[PluginEvent(ServerEventType.PlayerDropAmmo)]
		public void OnPlayerDropAmmo(Player player, ItemType ammo, int amount)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ammo",         ammo.ToString()                    },
				{ "amount",         amount.ToString()                    },
				{ "ipaddress",    player.IpAddress                   },
				{ "name",         player.Nickname                        },
				{ "playerid",     player.PlayerId.ToString()         },
				{ "steamid",      player.GetParsedUserID() },
				{ "class",        player.Role.ToString()    },
				{ "team",         player.ReferenceHub.GetTeam().ToString()    }
			};
			plugin.SendMessage("messages.onplayerdropammo", variables);
		}

		[PluginEvent(ServerEventType.PlayerDropItem)]
		public void OnPlayerDropItem(Player player, ItemBase item)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "item",         item.name                    },
				{ "ipaddress",    player.IpAddress                   },
				{ "name",         player.Nickname                        },
				{ "playerid",     player.PlayerId.ToString()         },
				{ "steamid",      player.GetParsedUserID() },
				{ "class",        player.Role.ToString()    },
				{ "team",         player.ReferenceHub.GetTeam().ToString()    }
			};
			plugin.SendMessage("messages.onplayerdropitem", variables);
		}

		[PluginEvent(ServerEventType.PlayerJoined)]
		public void OnPlayerJoin(Player player)
		{
			if (player.PlayerId == Server.Instance.PlayerId) return;

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress",    player.IpAddress                          },
				{ "name",         player.Nickname                           },
				{ "playerid",     player.PlayerId.ToString()                },
				{ "steamid",      player.GetParsedUserID()                  },
				{ "class",        player.Role.ToString()                    },
				{ "team",         player.ReferenceHub.GetTeam().ToString()  }
			};
			plugin.SendMessage("messages.onplayerjoin", variables);
		}

		[PluginEvent(ServerEventType.PlayerLeft)]
		public void OnPlayerLeave(Player player)
		{
			if (player.PlayerId == Server.Instance.PlayerId || player.UserId == null) return;

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress", player.IpAddress           },
				{ "name", player.Nickname                 },
				{ "steamid", player.GetParsedUserID()     },
				{ "playerid", player.PlayerId.ToString()  }
			};
			plugin.SendMessage("messages.onplayerleave", variables);
		}

		/*
		public void OnNicknameSet(PlayerNicknameSetEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "nickname",       ev.Nickname                         },
				{ "ipaddress",      ev.Player.IPAddress                 },
				{ "name",           ev.Player.Name                      },
				{ "playerid",       ev.Player.PlayerID.ToString()       },
				{ "steamid",        ev.Player.GetParsedUserID() ?? ev.Player.UserID },
				{ "class",          ev.Player.Role.ToString()  },
				{ "team",           ev.Player.ReferenceHub.GetTeam().ToString()  }
			};
			plugin.SendMessage(Config.GetArray("messages.onnicknameset"), "messages.onnicknameset", variables);
		}
		*/

		/*[PluginEvent(ServerEventType.PlayerChangeRole)]
		public void OnSetRole(Player player, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason changeReason)
		{
			if (newRole == RoleTypeId.None)
			{
				return;
			}

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "oldrole",        oldRole.RoleTypeId.ToString()            },
				{ "ipaddress",      player.IpAddress                         },
				{ "name",           player.Nickname                          },
				{ "playerid",       player.PlayerId.ToString()               },
				{ "steamid",        player.GetParsedUserID()                 },
				{ "class",          newRole.ToString()                       },
				{ "team",           player.ReferenceHub.GetTeam().ToString() }
			};

			switch (changeReason)
			{
				case RoleChangeReason.RoundStart:
					plugin.SendMessage("messages.onsetrole.roundstart", variables);
					return;
				case RoleChangeReason.LateJoin:
					if (newRole == RoleTypeId.Spectator)
					{
						plugin.SendMessage("messages.onsetrole.other", variables);
					}
					else
					{
						plugin.SendMessage("messages.onsetrole.latejoin", variables);
					}
					return;
				case RoleChangeReason.Respawn:
					plugin.SendMessage("messages.onsetrole.respawn", variables);
					return;
				case RoleChangeReason.Died:
					plugin.SendMessage("messages.onsetrole.died", variables);
					return;
				case RoleChangeReason.Escaped:
					plugin.SendMessage("messages.onsetrole.escaped", variables);
					return;
				case RoleChangeReason.Revived:
					plugin.SendMessage("messages.onsetrole.revived", variables);
					return;
				case RoleChangeReason.RemoteAdmin:
					plugin.SendMessage("messages.onsetrole.remoteadmin", variables);
					return;
				case RoleChangeReason.Destroyed:
					plugin.SendMessage("messages.onsetrole.left", variables);
					return;
				case RoleChangeReason.None:
					plugin.SendMessage("messages.onsetrole.other", variables);
					return;
				default:
					return;
			}
		}*/

		[PluginEvent(ServerEventType.PlayerSpawn)]
		public void OnSpawn(Player player, RoleTypeId role)
		{
			if (player == null || player.UserId == "server" || player.UserId == null || role == RoleTypeId.None || role == RoleTypeId.Spectator || role == RoleTypeId.Overwatch) return;

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ipaddress",      player.IpAddress                         },
				{ "name",           player.Nickname                          },
				{ "playerid",       player.PlayerId.ToString()               },
				{ "steamid",        player.GetParsedUserID()                 },
				{ "class",          role.ToString()                          },
				{ "team",           player.ReferenceHub.GetTeam().ToString() }
			};

			plugin.SendMessage("messages.onspawn", variables);
		}

		[PluginEvent]
		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "players", ev.Players.Select(x => x.Nickname).ToString() }
			};

			plugin.SendMessage(ev.Team == SpawnableTeamType.ChaosInsurgency ? "messages.onteamrespawn.ci" : "messages.onteamrespawn.mtf", variables);
		}

		[PluginEvent(ServerEventType.PlayerThrowProjectile)]
		public void OnThrowProjectile(Player player, ThrowableItem item, ThrowableItem.ProjectileSettings settings, bool fullForce)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "type",           item.ItemTypeId.ToString()               },
				{ "ipaddress",      player.IpAddress                         },
				{ "name",           player.Nickname                          },
				{ "playerid",       player.PlayerId.ToString()               },
				{ "steamid",        player.GetParsedUserID()                 },
				{ "class",          player.Role.ToString()                   },
				{ "team",           player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.onthrowprojectile", variables);
		}

		[PluginEvent(ServerEventType.RagdollSpawn)]
		public void OnSpawnRagdoll(Player player, IRagdollRole ragdollRole, DamageHandlerBase damageHandler)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "damagetype",         GetDamageType(damageHandler)              },
				{ "ipaddress",          player.IpAddress                          },
				{ "name",               player.Nickname                           },
				{ "playerid",           player.PlayerId.ToString()                },
				{ "steamid",            player.GetParsedUserID()                  },
				{ "class",              player.ToString()                         },
				{ "team",               player.ReferenceHub.GetTeam().ToString()  }
			};
			plugin.SendMessage("messages.onspawnragdoll", variables);
		}

		[PluginEvent(ServerEventType.PlayerUsedItem)]
		public void OnItemUse(Player player, ItemBase item)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "item",           item.ItemTypeId.ToString()               },
				{ "ipaddress",      player.IpAddress                         },
				{ "name",           player.Nickname                          },
				{ "playerid",       player.PlayerId.ToString()               },
				{ "steamid",        player.GetParsedUserID()                 },
				{ "class",          player.Role.ToString()                   },
				{ "team",           player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.onitemuse", variables);
		}

		[PluginEvent(ServerEventType.PlayerHandcuff)]
		public void OnHandcuffed(Player disarmer, Player target)
		{
			if (disarmer != null)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "targetipaddress",    target.IpAddress                           },
					{ "targetname",         target.Nickname                            },
					{ "targetplayerid",     target.PlayerId.ToString()                 },
					{ "targetsteamid",      target.GetParsedUserID()                   },
					{ "targetclass",        target.Role.ToString()                     },
					{ "targetteam",         target.ReferenceHub.GetTeam().ToString()   },
					{ "playeripaddress",    disarmer.IpAddress                         },
					{ "playername",         disarmer.Nickname                          },
					{ "playerplayerid",     disarmer.PlayerId.ToString()               },
					{ "playersteamid",      disarmer.GetParsedUserID()                 },
					{ "playerclass",        disarmer.Role.ToString()                   },
					{ "playerteam",         disarmer.ReferenceHub.GetTeam().ToString() }
				};
				plugin.SendMessage("messages.onhandcuff.default", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "targetipaddress",    target.IpAddress                           },
					{ "targetname",         target.Nickname                            },
					{ "targetplayerid",     target.PlayerId.ToString()                 },
					{ "targetsteamid",      target.GetParsedUserID()                   },
					{ "targetclass",        target.Role.ToString()                     },
					{ "targetteam",         target.ReferenceHub.GetTeam().ToString()   },
				};
				plugin.SendMessage("messages.onhandcuff.nootherplayer", variables);
			}
		}

		[PluginEvent]
		public void OnHandcuffsRemoved(PlayerRemoveHandcuffsEvent ev)
		{
			if (ev.Player != null)
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "targetipaddress",    ev.Target.IpAddress                           },
					{ "targetname",         ev.Target.Nickname                            },
					{ "targetplayerid",     ev.Target.PlayerId.ToString()                 },
					{ "targetsteamid",      ev.Target.GetParsedUserID()                   },
					{ "targetclass",        ev.Target.Role.ToString()                     },
					{ "targetteam",         ev.Target.ReferenceHub.GetTeam().ToString()   },
					{ "playeripaddress",    ev.Player.IpAddress                         },
					{ "playername",         ev.Player.Nickname                          },
					{ "playerplayerid",     ev.Player.PlayerId.ToString()               },
					{ "playersteamid",      ev.Player.GetParsedUserID()                 },
					{ "playerclass",        ev.Player.Role.ToString()                   },
					{ "playerteam",         ev.Player.ReferenceHub.GetTeam().ToString() }
				};
				plugin.SendMessage("messages.onhandcuffremoved.default", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string>
				{
					{ "targetipaddress",    ev.Target.IpAddress                           },
					{ "targetname",         ev.Target.Nickname                            },
					{ "targetplayerid",     ev.Target.PlayerId.ToString()                 },
					{ "targetsteamid",      ev.Target.GetParsedUserID()                   },
					{ "targetclass",        ev.Target.Role.ToString()                     },
					{ "targetteam",         ev.Target.ReferenceHub.GetTeam().ToString()   },
				};
				plugin.SendMessage("messages.onhandcuffremoved.nootherplayer", variables);
			}
		}

		[PluginEvent]
		public void OnPlayerChangeRadioRange(PlayerChangeRadioRangeEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "setting",        ev.Range.ToString()                         },
				{ "ipaddress",      ev.Player.IpAddress                         },
				{ "name",           ev.Player.Nickname                          },
				{ "playerid",       ev.Player.PlayerId.ToString()               },
				{ "steamid",        ev.Player.GetParsedUserID()                 },
				{ "class",          ev.Player.Role.ToString()                   },
				{ "team",           ev.Player.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.onplayerradioswitch", variables);
		}

		[PluginEvent(ServerEventType.PlayerReloadWeapon)]
		public void OnReload(Player player, Firearm weapon)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "weapon",      weapon.name                                 },
				{ "maxclipsize", weapon.AmmoManagerModule.MaxAmmo.ToString() },
				{ "ipaddress",   player?.IpAddress                           },
				{ "name",        player?.Nickname                            },
				{ "playerid",    player?.PlayerId.ToString()                 },
				{ "steamid",     player?.GetParsedUserID()                   },
				{ "class",       player?.Role.ToString()                     },
				{ "team",        player?.ReferenceHub.GetTeam().ToString()   }
			};
			plugin.SendMessage("messages.onreload", variables);
		}

		[PluginEvent(ServerEventType.GrenadeExploded)]
		public void OnGrenadeExplosion(Footprint footprint, Vector3 position, ItemPickupBase grenade)
		{
			Player player = new Player(footprint.Hub);
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "type",        grenade.name                                },
				{ "ipaddress",   player?.IpAddress                           },
				{ "name",        player?.Nickname                            },
				{ "playerid",    player?.PlayerId.ToString()                 },
				{ "steamid",     player?.GetParsedUserID()                   },
				{ "class",       player?.Role.ToString()                     },
				{ "team",        player?.ReferenceHub.GetTeam().ToString()   }
			};
			plugin.SendMessage("messages.ongrenadeexplosion", variables);
		}

		/*
		public void OnGrenadeHitPlayer(PlayerGrenadeHitPlayer ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "playeripaddress",    ev.Player?.IPAddress                 },
				{ "playername",         ev.Player?.Name                      },
				{ "playerplayerid",     ev.Player?.PlayerID.ToString()       },
				{ "playersteamid",      ev.Player?.GetParsedUserID() ?? ev.Player?.UserID },
				{ "playerclass",        ev.Player?.PlayerRole?.RoleID.ToString()    },
				{ "playerteam",         ev.Player?.PlayerRole?.Team.ToString()  },
				{ "targetipaddress",    ev.Victim?.IPAddress                 },
				{ "targetname",         ev.Victim?.Name                      },
				{ "targetplayerid",     ev.Victim?.PlayerID.ToString()       },
				{ "targetsteamid",      ev.Victim?.GetParsedUserID() ?? ev.Victim?.UserID },
				{ "targetclass",        ev.Victim?.Role.ToString()    },
				{ "targetteam",         ev.Victim?.ReferenceHub.GetTeam().ToString()  },
			};
			plugin.SendMessage(Config.GetArray("messages.ongrenadehitplayer"), "messages.ongrenadehitplayer", variables);
		}
		*/

		[PluginEvent(ServerEventType.PlayerEscape)]
		public void OnPlayerEscape(Player player, RoleTypeId newRole)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "newclass",  newRole.ToString()                        },
				{ "ipaddress", player?.IpAddress                         },
				{ "name",      player?.Nickname                          },
				{ "playerid",  player?.PlayerId.ToString()               },
				{ "steamid",   player?.GetParsedUserID()                 },
				{ "oldclass",  player?.Role.ToString()                   },
				{ "team",      player?.ReferenceHub.GetTeam().ToString() }
			};
			plugin.SendMessage("messages.onplayerescape", variables);
		}
	}
}