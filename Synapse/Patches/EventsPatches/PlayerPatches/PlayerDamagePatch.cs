using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Dissonance.Integrations.MirrorIgnorance;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.MicroHID;
using MapGeneration;
using PlayableScps.Interfaces;
using Respawning;
using Synapse.Api;
using UnityEngine;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
	internal static class PlayerDamagePatch
	{
		[HarmonyPrefix]
		private static bool HurtPlayer(PlayerStats __instance, out bool __result, PlayerStats.HitInfo info, GameObject go, bool noTeamDamage = false, bool IsValidDamage = true)
		{
			try
			{
				__result = false;
				var victim = go?.GetPlayer();
				var killer = __instance?.GetPlayer();
				if (victim == null) return false;

				if (info.Tool == DamageTypes.Grenade)
					killer = SynapseController.Server.GetPlayer(info.PlayerId);
				else if (info.Tool == DamageTypes.Pocket)
                {
					killer = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(victim));

					if (!SynapseExtensions.CanHarmScp(victim, false))
						return false;
                }

				bool flag = false;
				bool flag2 = false;
				bool flag3 = go == null;
				var referenceHub = flag3 ? null : ReferenceHub.GetHub(go);
				var damageType = info.Tool;

				if (info.Amount < 0f)
				{
					if (flag3)
						info.Amount = Mathf.Abs(999999f);
					else
						info.Amount = ((referenceHub.playerStats != null) ? Mathf.Abs(referenceHub.playerStats.Health + referenceHub.playerStats.ArtificialHealth + 10f) : Mathf.Abs(999999f));
				}

				if(referenceHub != null)
                {
					var effect = referenceHub.playerEffectsController.GetEffect<CustomPlayerEffects.Burned>();
					if (effect != null && effect.IsEnabled)
						info.Amount *= effect.damageMultiplier;
                }

				if (info.Amount > 2.14748365E+09f)
					info.Amount = 2.14748365E+09f;

				if (flag3)
					return false;

				PlayerStats playerStats = referenceHub.playerStats;
				CharacterClassManager characterClassManager = referenceHub.characterClassManager;

				if (playerStats == null || characterClassManager == null)
					return false;

				if (characterClassManager.GodMode)
					return false;

				if (__instance.ccm.CurRole.team == Team.SCP && __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team == Team.SCP && __instance.ccm != characterClassManager)
					return false;

				if (characterClassManager.SpawnProtected && !__instance._allowSPDmg)
					return false;

				bool flag4 = !noTeamDamage && info.IsPlayer && referenceHub != info.RHub && referenceHub.characterClassManager.Faction == info.RHub.characterClassManager.Faction;

				if (flag4)
					info.Amount *= PlayerStats.FriendlyFireFactor;

				float health = playerStats.Health;
				var num = playerStats.GetAhpValue();

				var allow = true;
				try
				{
					Server.Get.Events.Player.InvokePlayerDamageEvent(victim, killer, ref info, out allow);
				}
				catch (Exception e)
				{
					SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage Event failed!!\n{e}");
				}

				if (!allow)
				{
					__result = false;
					return false;
				}

				if (num > 0f)
				{
					float num2 = info.Amount * playerStats.ArtificialNormalRatio;
					float num3 = info.Amount - num2;
					num -= num2;
					if (num < 0f)
					{
						num3 += Mathf.Abs(num);
					}
					playerStats.SafeSetAhpValue(num);
					playerStats.Health -= num3;
					if (playerStats.Health > 0f && playerStats.Health - num2 <= 0f && characterClassManager.CurRole.team != Team.SCP)
					{
						__instance.TargetAchieve(characterClassManager.connectionToClient, "didntevenfeelthat");
					}
				}
				else
					playerStats.Health -= info.Amount;

				if (playerStats.Health < 0f)
					playerStats.Health = 0f;

				playerStats.lastHitInfo = info;

				PlayableScpsController component = go.GetComponent<PlayableScpsController>();
				if (component != null && (object)component.CurrentScp is PlayableScps.Interfaces.IDamagable damagable)
					damagable.OnDamage(info);

				RespawnTickets singleton = RespawnTickets.Singleton;
				if (characterClassManager.CurRole.team == Team.SCP && characterClassManager.CurRole.roleId != RoleType.Scp0492)
				{
					if (characterClassManager.CurRole.roleId != RoleType.Scp079)
					{
						for (float num4 = 1f; num4 > 0f; num4 -= __instance._respawn_tickets_mtf_scp_hurt_interval)
						{
							float num5 = (float)playerStats.maxHP * num4;
							if (health > num5 && playerStats.Health < num5)
							{
								singleton.GrantTickets(SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_hurt_count, false);
							}
						}
					}
					if (playerStats.Health < 1f)
					{
						singleton.GrantTickets(SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_death_count, false);
					}
				}

				byte damageTypeId = (byte)DamageTypes.ToIndex(damageType);
				foreach (KeyValuePair<Type, PlayerEffect> keyValuePair in __instance.Hub.playerEffectsController.AllEffects)
				{
					PlayerEffect value = keyValuePair.Value;
					if (value.IsEnabled && (object)value is IOnDamageTakenEffect effect && effect.AllowPulse(damageType))
					{
						__instance.RpcTriggerPostProcessing(damageTypeId);
					}
				}

				if (playerStats.Health < 1f && characterClassManager.CurClass != RoleType.Spectator)
				{
					if (component != null && (object)component.CurrentScp is PlayableScps.Interfaces.IImmortalScp immortalScp && !immortalScp.OnDeath(info, __instance.gameObject))
					{
						__result = false;
						return false;
					}

					foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
					{
						bool flag5 = false;
						using (IEnumerator<Scp079Interaction> enumerator3 = scp079PlayerScript.ReturnRecentHistory(12f, __instance._filters).GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								if (RoomIdUtils.IsTheSameRoom(enumerator3.Current.interactable.transform.position, go.transform.position))
								{
									flag5 = true;
								}
							}
						}
						if (flag5)
						{
							scp079PlayerScript.RpcGainExp(ExpGainType.KillAssist, characterClassManager.CurClass);
						}
					}

					if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60 && IsValidDamage)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "wowreally");

					if (__instance.isLocalPlayer && info.PlayerId != referenceHub.queryProcessor.PlayerId)
						RoundSummary.Kills++;

					flag = true;

					if(info.RHub != null && info.RHub.Ready && IsValidDamage)
						GameplayTickets.Singleton.TicketBasedKilling(characterClassManager.CurRole.team, info.RHub.characterClassManager.CurRole.team);

					if (characterClassManager.CurClass == RoleType.Scp096)
					{
						ReferenceHub hub = ReferenceHub.GetHub(go);

						if (hub != null && hub.scpsController.CurrentScp is PlayableScps.Scp096 && (hub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState == PlayableScps.Scp096PlayerState.Enraging)
							__instance.TargetAchieve(characterClassManager.connectionToClient, "unvoluntaryragequit");
					}
					else if (info.Tool == DamageTypes.Pocket)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "newb");
					else if (info.Tool == DamageTypes.Scp173)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "firsttime");
					else if (info.Tool == DamageTypes.Grenade && info.PlayerId == referenceHub.queryProcessor.PlayerId)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "iwanttobearocket");
					else if (info.Tool.Weapon != ItemType.None)
					{
						var inventory = referenceHub.inventory;
						if (characterClassManager.CurClass == RoleType.Scientist && inventory.CurInstance != null &&
							inventory.CurInstance.Category == ItemCategory.Keycard &&
							__instance.GetComponent<CharacterClassManager>().CurClass == RoleType.ClassD)
						{
								__instance.TargetAchieve(__instance.connectionToClient, "betrayal");
						}

						if (Time.realtimeSinceStartup - __instance._killStreakTime > 30f || __instance._killStreak == 0)
						{
							__instance._killStreak = 0;
							__instance._killStreakTime = Time.realtimeSinceStartup;
						}

						if (HitboxIdentity.CheckFriendlyFire(__instance.Hub, referenceHub, false))
							__instance._killStreak++;

						if (__instance._killStreak >= 5)
							__instance.TargetAchieve(__instance.connectionToClient, "pewpew");

						if ((__instance.ccm.CurRole.team == Team.MTF || __instance.ccm.Classes.SafeGet(__instance.ccm.CurClass).team == Team.RSC) && characterClassManager.CurClass == RoleType.ClassD)
							__instance.TargetStats(__instance.connectionToClient, "dboys_killed", "justresources", 50);

					}
					else if (__instance.ccm.CurRole.team == Team.SCP && referenceHub.inventory.CurInstance != null && referenceHub.inventory.CurInstance is MicroHIDItem microHIDItem && microHIDItem != null && microHIDItem.State != HidState.Idle)
					{
						__instance.TargetAchieve(__instance.connectionToClient, "illpassthanks");
					}

					Team team = characterClassManager.CurRole.team;
					byte b = (byte)team;
					if (b == 3 && flag && info.RHub != null)
					{
						Team team2 = __instance.ccm.Classes.SafeGet(info.RHub.characterClassManager.CurClass).team;
						if (team2 == Team.CDP || team2 == Team.CHI)
						{
							singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, __instance._respawn_tickets_ci_scientist_died_count, false);
						}
					}

					if (victim.RealTeam == Team.RSC && victim.RealTeam == Team.SCP)
						__instance.TargetAchieve(__instance.connectionToClient, "timetodoitmyself");

					bool flag6 = info.IsPlayer && referenceHub == info.RHub;
					flag2 = flag4;

					if (flag6)
					{
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
						{
					referenceHub.LoggedNameFromRefHub(),
					" playing as ",
					referenceHub.characterClassManager.CurRole.fullName,
					" committed a suicide using ",
					info.Tool.Name,
					"."
						}), ServerLogs.ServerLogType.Suicide, false);
					}
					else
					{
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
						{
					referenceHub.LoggedNameFromRefHub(),
					" playing as ",
					referenceHub.characterClassManager.CurRole.fullName,
					" has been killed by ",
					info.Attacker,
					" using ",
					info.Tool.Name,
					info.IsPlayer ? (" playing as " + info.RHub.characterClassManager.CurRole.fullName + ".") : "."
						}), flag2 ? ServerLogs.ServerLogType.Teamkill : ServerLogs.ServerLogType.KillLog, false);
					}

					if (info.Tool.Scp != RoleType.None || info.Tool == DamageTypes.Pocket)
						RoundSummary.kills_by_scp++;

					else if (info.Tool == DamageTypes.Grenade)
						RoundSummary.kills_by_frag++;

					try
					{
						Server.Get.Events.Player.InvokePlayerDeathEvent(victim, killer, info);
					}

					catch (Exception e)
					{
						SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDeath Event failed!!\n{e}");
					}

					if (!__instance._pocketCleanup || info.Tool != DamageTypes.Pocket)
					{
						referenceHub.inventory.ServerDropEverything();
                        bool flag7 = __instance.TryGet096SmackVelocity(info.RHub, referenceHub, out Vector3 vector);
                        Vector3 vector2 = flag7 ? vector : referenceHub.playerMovementSync.PlayerVelocity;
						if (characterClassManager.Classes.CheckBounds(characterClassManager.CurClass) && damageType != DamageTypes.RagdollLess)
						{
							__instance.GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, (referenceHub.playerMovementSync == null) ? Vector3.zero : vector2, (int)characterClassManager.CurClass, info, characterClassManager.CurRole.team > Team.SCP, go.GetComponent<MirrorIgnorancePlayer>().PlayerId, referenceHub.nicknameSync.DisplayName, referenceHub.queryProcessor.PlayerId, flag7);
						}
					}
					characterClassManager.DeathPosition = go.transform.position;

					if (characterClassManager.CurRole.team == Team.SCP && characterClassManager.CurClass != RoleType.Scp0492)
					{
						GameObject x = null;
						foreach (GameObject gameObject in PlayerManager.players)
						{
							if (gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId == info.PlayerId)
								x = gameObject;
						}
						if (x != null)
							NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, string.Empty);
						else
						{
							DamageTypes.DamageType damageType2 = info.Tool;
							if (damageType2 == DamageTypes.Tesla)
								NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "TESLA");
							else if (damageType2 == DamageTypes.Nuke)
								NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "WARHEAD");
							else if (damageType2 == DamageTypes.Decont)
								NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "DECONTAMINATION");
							else if (characterClassManager.CurClass != RoleType.Scp079)
								NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "UNKNOWN");
						}
					}

					playerStats.SetHPAmount(100);
					characterClassManager.SetClassID(RoleType.Spectator, CharacterClassManager.SpawnReason.Died);

					victim.CustomRole = null;
					foreach (var larry in Server.Get.Players.Where(x => x.Scp106Controller.PocketPlayers.Contains(victim)))
						larry.Scp106Controller.PocketPlayers.Remove(victim);

					if (victim.IsDummy)
						Map.Get.Dummies.FirstOrDefault(x => x.Player == victim).Destroy();
				}
				else
				{
					Vector3 pos = Vector3.zero;
					if (info.Tool.Weapon != ItemType.None)
					{
						GameObject playerOfID = __instance.GetPlayerOfID(info.PlayerId);
						if (playerOfID != null)
						{
							pos = go.transform.InverseTransformPoint(playerOfID.transform.position).normalized;
						}
					}
					else if (info.Tool == DamageTypes.Pocket)
					{
						PlayerMovementSync component2 = __instance.ccm.GetComponent<PlayerMovementSync>();
						if (component2.RealModelPosition.y > -1900f)
							component2.OverridePosition(Vector3.down * 1998.5f, 0f, true);
					}
				}

				if (component != null && (object)component.CurrentScp is IDamagable damagable2 && damagable2 != null)
				{
					damagable2.OnDamage(info);
				}

				__result = flag;

				if (!flag4 || FriendlyFireConfig.PauseDetector || PermissionsHandler.IsPermitted(info.RHub.serverRoles.Permissions, PlayerPermissions.FriendlyFireDetectorImmunity))
				{
					return false;
				}
				if (FriendlyFireConfig.IgnoreClassDTeamkills && referenceHub.characterClassManager.CurRole.team == Team.CDP && info.RHub.characterClassManager.CurRole.team == Team.CDP)
				{
					return false;
				}
				if (flag2)
				{
					if (info.RHub.FriendlyFireHandler.Respawn.RegisterKill())
					{
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Window.RegisterKill())
					{
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Life.RegisterKill())
					{
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Round.RegisterKill())
					{
						return false;
					}
				}
				if (info.RHub.FriendlyFireHandler.Respawn.RegisterDamage(info.Amount))
				{
					return false;
				}
				if (info.RHub.FriendlyFireHandler.Window.RegisterDamage(info.Amount))
				{
					return false;
				}
				if (info.RHub.FriendlyFireHandler.Life.RegisterDamage(info.Amount))
				{
					return false;
				}
				info.RHub.FriendlyFireHandler.Round.RegisterDamage(info.Amount);

				return false;
			}
			catch (Exception e)
			{
				SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage Patch failed!!\n{e}");
				__result = false;
				return true;
			}
		}
	}
}