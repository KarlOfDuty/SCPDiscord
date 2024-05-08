using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using Respawning;

namespace SCPDiscord.EventListeners
{
	internal class PlayerEventListener
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


        [PluginEvent]
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
					{ "damage",     stdHandler.Damage.ToString("0.##") },
					{ "damagetype", GetDamageType(ev.DamageHandler)    }
				};
				noAttackerVar.AddPlayerVariables(ev.Target, "target");

				plugin.SendMessage("messages.onplayerhurt.noattacker", noAttackerVar);
				return;
			}

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "damage",     stdHandler.Damage.ToString("0.##") },
				{ "damagetype", GetDamageType(ev.DamageHandler)    }
			};

			variables.AddPlayerVariables(ev.Target, "target");
			variables.AddPlayerVariables(ev.Player, "attacker");

			if (IsTeamDamage(ev.Player.ReferenceHub.GetTeam(), ev.Target.ReferenceHub.GetTeam()))
			{
				plugin.SendMessage("messages.onplayerhurt.friendlyfire", variables);
				return;
			}

			plugin.SendMessage("messages.onplayerhurt.default", variables);
		}

		[PluginEvent]
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
					{ "damagetype", GetDamageType(ev.DamageHandler) }
				};
				noKillerVar.AddPlayerVariables(ev.Player, "target");
				plugin.SendMessage("messages.onplayerdie.nokiller", noKillerVar);
				return;
			}

			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "damagetype", GetDamageType(ev.DamageHandler) }
			};
			variables.AddPlayerVariables(ev.Attacker, "attacker");
			variables.AddPlayerVariables(ev.Player, "target");

			if (IsTeamDamage(ev.Attacker.ReferenceHub.GetTeam(), ev.Player.ReferenceHub.GetTeam()))
			{
				plugin.SendMessage("messages.onplayerdie.friendlyfire", variables);
				return;
			}
			plugin.SendMessage("messages.onplayerdie.default", variables);
		}

		[PluginEvent]
		public void OnPlayerPickupAmmo(PlayerPickupAmmoEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ammo", ev.Item.Info.ItemId.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");

			plugin.SendMessage("messages.onplayerpickupammo", variables);
		}

		[PluginEvent]
		public void OnPlayerPickupArmor(PlayerPickupArmorEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "armor", ev.Item.Info.ItemId.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerpickuparmor", variables);
		}

		[PluginEvent]
		public void OnPlayerPickupSCP330(PlayerPickupScp330Event ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerpickupscp330", variables);
		}

		[PluginEvent]
		public void OnPlayerPickupItem(PlayerSearchedPickupEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "item", ev.Item.Info.ItemId.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerpickupitem", variables);
		}

		[PluginEvent]
		public void OnPlayerDropAmmo(PlayerDropAmmoEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "ammo",   ev.Item.ToString()   },
				{ "amount", ev.Amount.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerdropammo", variables);
		}

		[PluginEvent]
		public void OnPlayerDropItem(PlayerDropItemEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "item", ev.Item.ItemTypeId.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerdropitem", variables);
		}

		[PluginEvent]
		public void OnPlayerJoin(PlayerJoinedEvent ev)
		{
			if (ev.Player.PlayerId == Server.Instance.PlayerId) return;

			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerjoin", variables);
		}

		[PluginEvent]
		public void OnPlayerLeave(PlayerLeftEvent ev)
		{
			if (ev.Player.PlayerId == Server.Instance.PlayerId || ev.Player.UserId == null) return;

			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerleave", variables);
		}

		[PluginEvent]
		public void OnSpawn(PlayerSpawnEvent ev)
		{
			if (ev.Player == null
			 || ev.Player.UserId == "server"
			 || ev.Player.UserId == null
			 || ev.Role == RoleTypeId.None
			 || ev.Role == RoleTypeId.Spectator
			 || ev.Role == RoleTypeId.Overwatch)
				return;

			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Player, "player");

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

		[PluginEvent]
		public void OnThrowProjectile(PlayerThrowProjectileEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "type", ev.Item.ItemTypeId.ToString() }
			};
			variables.AddPlayerVariables(ev.Thrower, "player");
			plugin.SendMessage("messages.onthrowprojectile", variables);
		}

		[PluginEvent]
		public void OnSpawnRagdoll(RagdollSpawnEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "damagetype", GetDamageType(ev.DamageHandler) }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onspawnragdoll", variables);
		}

		[PluginEvent]
		public void OnItemUse(PlayerUsedItemEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "item", ev.Item.ItemTypeId.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onitemuse", variables);
		}

		[PluginEvent]
		public void OnHandcuffed(PlayerHandcuffEvent ev)
		{
			if (ev.Player == null)
			{
				Dictionary<string, string> noOtherPlayerVars = new Dictionary<string, string> {};
				noOtherPlayerVars.AddPlayerVariables(ev.Target, "target");
				plugin.SendMessage("messages.onhandcuff.nootherplayer", noOtherPlayerVars);
				return;
			}

			Dictionary<string, string> variables = new Dictionary<string, string> {};
			variables.AddPlayerVariables(ev.Target, "target");
			variables.AddPlayerVariables(ev.Player, "disarmer");
			plugin.SendMessage("messages.onhandcuff.default", variables);
		}

		[PluginEvent]
		public void OnHandcuffsRemoved(PlayerRemoveHandcuffsEvent ev)
		{
			if (ev.Player != null)
			{
				Dictionary<string, string> variables = new Dictionary<string, string> {};
				variables.AddPlayerVariables(ev.Target, "target");
				variables.AddPlayerVariables(ev.Player, "disarmer");
				plugin.SendMessage("messages.onhandcuffremoved.default", variables);
			}
			else
			{
				Dictionary<string, string> variables = new Dictionary<string, string> {};
				variables.AddPlayerVariables(ev.Target, "target");
				plugin.SendMessage("messages.onhandcuffremoved.nootherplayer", variables);
			}
		}

		[PluginEvent]
		public void OnPlayerChangeRadioRange(PlayerChangeRadioRangeEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "setting", ev.Range.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerradioswitch", variables);
		}

		[PluginEvent]
		public void OnReload(PlayerReloadWeaponEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "weapon",      ev.Firearm.ItemTypeId.ToString()                },
				{ "maxclipsize", ev.Firearm.AmmoManagerModule.MaxAmmo.ToString() }
			};
			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onreload", variables);
		}

		[PluginEvent]
		public void OnGrenadeExplosion(GrenadeExplodedEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "type", ev?.Grenade.Info.ItemId.ToString() }
			};

			if (ev?.Thrower.Hub != null)
				variables.AddPlayerVariables(new Player(ev.Thrower.Hub), "player");

			plugin.SendMessage("messages.ongrenadeexplosion", variables);
		}

		[PluginEvent]
		public void OnPlayerEscape(PlayerEscapeEvent ev)
		{
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "newrole", ev.NewRole.ToString() }
			};

			variables.AddPlayerVariables(ev.Player, "player");
			plugin.SendMessage("messages.onplayerescape", variables);
		}

		[PluginEvent]
        public void OnPlayerAim(PlayerAimWeaponEvent ev)
		{
            Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "weapon",   ev.Firearm.ItemTypeId.ToString() },
				{ "isAiming", ev.IsAiming.ToString() }
			};
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.onplayeraim", variables);
        }

		[PluginEvent]
        public void OnPlayerCancelUsingItem(PlayerCancelUsingItemEvent ev)
		{
            Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "item", ev.Item.ItemTypeId.ToString() }
			};
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.onplayercancelusingitem", variables);
        }

		[PluginEvent]
        public void OnPlayerChanceItem(PlayerChangeItemEvent ev)
		{
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "oldItem", ev.OldItem.ToString() },
                { "newItem", ev.NewItem.ToString() },
            };
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.onplayerchangeitem", variables);
        }

		[PluginEvent]
        public void OnPlayerChangeSpectator(PlayerChangeSpectatorEvent ev)
        {
			Dictionary<string, string> variables = new Dictionary<string, string> { };
            variables.AddPlayerVariables(ev.OldTarget, "oldTarget");
            variables.AddPlayerVariables(ev.NewTarget, "newTarget");
            variables.AddPlayerVariables(ev.Player, "spectator");

            plugin.SendMessage("messages.onplayerchangespectator", variables);
        }

		[PluginEvent]
        public void OnPlayerDamageShootingTarget(PlayerDamagedShootingTargetEvent ev)
        {
			Dictionary<string, string> variables = new Dictionary<string, string>
			{
				{ "amount", ev.DamageAmount.ToString("0.##") },
				{ "type",   GetDamageType(ev.DamageHandler) },
				{ "target", ev.ShootingTarget.ToString() }
            };
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.onplayerdamageshootingtarget", variables);
        }

		[PluginEvent]
        public void OnPlayerDamageWindow(PlayerDamagedWindowEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "amount", ev.DamageAmount.ToString("0.##") },
                { "type",   GetDamageType(ev.DamageHandler) },
                //{ "window", ev.Window.ToString() }
            };
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.onplayerdamagewindow", variables);
        }

		[PluginEvent]
        public void OnPlayerDryfireWeapon(PlayerDryfireWeaponEvent ev)
		{
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "weapon", ev.Firearm.ItemTypeId.ToString() }
            };
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.onplayerdryfireweapon", variables);
        }

		[PluginEvent]
        public void OnPlayerrReceiveEffect(PlayerReceiveEffectEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "effect", ev.Effect.ToString() },
                { "duration", ev.Duration.ToString() },
                { "intensity", ev.Intensity.ToString() }
            };
            variables.AddPlayerVariables(ev.Player, "player");

            plugin.SendMessage("messages.onplayerreceiveeffect", variables);
        }
    }
}