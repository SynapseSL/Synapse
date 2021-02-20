using System;
using System.Linq;
using HarmonyLib;
using Synapse.Api;
using UnityEngine;
using Mirror;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
	[HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
	internal static class PlayerDamagePatch
	{
		private static bool Prefix(PlayerStats __instance, out bool __result, PlayerStats.HitInfo info, GameObject go, bool noTeamDamage = false, bool IsValidDamage = true)
		{
			try
			{
				__result = false;
				var victim = go?.GetPlayer();
				var killer = __instance?.GetPlayer();
				if (victim == null) return false;

				if (info.GetDamageType() == DamageTypes.Grenade)
					killer = SynapseController.Server.GetPlayer(info.PlayerId);
				else if (info.GetDamageType() == DamageTypes.Pocket)
                {
					killer = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(victim));

					if (!SynapseExtensions.CanHarmScp(victim, false))
						return false;
                }

				bool flag = false;
				bool flag2 = false;
				bool flag3 = go == null;
				ReferenceHub referenceHub = flag3 ? null : ReferenceHub.GetHub(go);

				if (info.Amount < 0f)
				{
					if (flag3)
						info.Amount = Mathf.Abs(999999f);
					else
						info.Amount = ((referenceHub.playerStats != null) ? Mathf.Abs(referenceHub.playerStats.Health + referenceHub.playerStats.syncArtificialHealth + 10f) : Mathf.Abs(999999f));
				}

				if (__instance._burned.Enabled)
					info.Amount *= __instance._burned.DamageMult;

				if (info.Amount > 2.14748365E+09f)
					info.Amount = 2.14748365E+09f;

				if (info.GetDamageType().isWeapon && referenceHub.characterClassManager.IsAnyScp() && info.GetDamageType() != DamageTypes.MicroHid)
					info.Amount *= __instance.weaponManager.weapons[(int)__instance.weaponManager.curWeapon].scpDamageMultiplier;

				if (flag3)
				{
					__result = false; 
					return false;
				}

				PlayerStats playerStats = referenceHub.playerStats;
				CharacterClassManager characterClassManager = referenceHub.characterClassManager;

				if (playerStats == null || characterClassManager == null)
				{
					__result = false; 
					return false;
				}

				if (characterClassManager.GodMode)
				{
					__result = false; 
					return false;
				}

				if (__instance.ccm.CurRole.team == Team.SCP && __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team == Team.SCP && __instance.ccm != characterClassManager)
				{
					__result = false; 
					return false;
				}

				if (characterClassManager.SpawnProtected && !__instance._allowSPDmg)
				{
					__result = false; 
					return false;
				}

				bool flag4 = !noTeamDamage && info.IsPlayer && referenceHub != info.RHub && referenceHub.characterClassManager.Fraction == info.RHub.characterClassManager.Fraction;

				if (flag4)
					info.Amount *= PlayerStats.FriendlyFireFactor;

				float health = playerStats.Health;

				if (__instance.lastHitInfo.Attacker == "ARTIFICIALDEGEN")
				{
					playerStats.unsyncedArtificialHealth -= info.Amount;
					if (playerStats.unsyncedArtificialHealth < 0f)
						playerStats.unsyncedArtificialHealth = 0f;
				}
				else
				{
					var allow = true;
					try
					{
						Server.Get.Events.Player.InvokePlayerDamageEvent(victim, killer, ref info, out allow);
					}
					catch(Exception e)
                    {
						SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage Event failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
					}

                    if (!allow)
                    {
						__result = false;
						return false;
                    }

					if (playerStats.unsyncedArtificialHealth > 0f)
					{
						float num = info.Amount * playerStats.artificialNormalRatio;
						float num2 = info.Amount - num;
						playerStats.unsyncedArtificialHealth -= num;
						if (playerStats.unsyncedArtificialHealth < 0f)
						{
							num2 += Mathf.Abs(playerStats.unsyncedArtificialHealth);
							playerStats.unsyncedArtificialHealth = 0f;
						}
						playerStats.Health -= num2;
						if (playerStats.Health > 0f && playerStats.Health - num <= 0f && characterClassManager.CurRole.team != Team.SCP)
						{
							__instance.TargetAchieve(characterClassManager.connectionToClient, "didntevenfeelthat");
						}
					}
					else
						playerStats.Health -= info.Amount;

					if (playerStats.Health < 0f)
						playerStats.Health = 0f;

					playerStats.lastHitInfo = info;
				}
				PlayableScpsController component = go.GetComponent<PlayableScpsController>();
				if (component != null && (object)component.CurrentScp is PlayableScps.Interfaces.IDamagable damagable)
					damagable.OnDamage(info);

				if (playerStats.Health < 1f && characterClassManager.CurClass != RoleType.Spectator)
				{
					if (component != null && (object)component.CurrentScp is PlayableScps.Interfaces.IImmortalScp immortalScp && !immortalScp.OnDeath(info, __instance.gameObject))
					{
						__result = false;
						return false;
					}
					foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
					{
						Scp079Interactable.ZoneAndRoom otherRoom = go.GetComponent<Scp079PlayerScript>().GetOtherRoom();
						bool flag5 = false;
						foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, __instance._filters))
							foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
								if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
									flag5 = true;

						if (flag5)
							scp079PlayerScript.RpcGainExp(ExpGainType.KillAssist, characterClassManager.CurClass);
					}

					if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60 && IsValidDamage)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "wowreally");

					if (__instance.isLocalPlayer && info.PlayerId != referenceHub.queryProcessor.PlayerId)
						RoundSummary.Kills++;

					flag = true;

					if (characterClassManager.CurClass == RoleType.Scp096)
					{
						ReferenceHub hub = ReferenceHub.GetHub(go);

						if (hub != null && hub.scpsController.CurrentScp is PlayableScps.Scp096 && (hub.scpsController.CurrentScp as PlayableScps.Scp096).PlayerState == PlayableScps.Scp096PlayerState.Enraging)
							__instance.TargetAchieve(characterClassManager.connectionToClient, "unvoluntaryragequit");
					}
					else if (info.GetDamageType() == DamageTypes.Pocket)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "newb");
					else if (info.GetDamageType() == DamageTypes.Scp173)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "firsttime");
					else if (info.GetDamageType() == DamageTypes.Grenade && info.PlayerId == referenceHub.queryProcessor.PlayerId)
						__instance.TargetAchieve(characterClassManager.connectionToClient, "iwanttobearocket");
					else if (info.GetDamageType().isWeapon)
					{
						Inventory inventory = referenceHub.inventory;
						if (characterClassManager.CurClass == RoleType.Scientist)
						{
							Item itemByID = inventory.GetItemByID(inventory.curItem);
							if (itemByID != null && itemByID.itemCategory == ItemCategory.Keycard && __instance.GetComponent<CharacterClassManager>().CurClass == RoleType.ClassD)
								__instance.TargetAchieve(__instance.connectionToClient, "betrayal");
						}

						if (Time.realtimeSinceStartup - __instance._killStreakTime > 30f || __instance._killStreak == 0)
						{
							__instance._killStreak = 0;
							__instance._killStreakTime = Time.realtimeSinceStartup;
						}

						if (__instance.GetComponent<WeaponManager>().GetShootPermission(characterClassManager, true))
							__instance._killStreak++;

						if (__instance._killStreak >= 5)
							__instance.TargetAchieve(__instance.connectionToClient, "pewpew");

						if ((__instance.ccm.CurRole.team == Team.MTF || __instance.ccm.Classes.SafeGet(__instance.ccm.CurClass).team == Team.RSC) && characterClassManager.CurClass == RoleType.ClassD)
							__instance.TargetStats(__instance.connectionToClient, "dboys_killed", "justresources", 50);

					}
					else if (__instance.ccm.CurRole.team == Team.SCP && go.GetComponent<MicroHID>().CurrentHidState != MicroHID.MicroHidState.Idle)
						__instance.TargetAchieve(__instance.connectionToClient, "illpassthanks");

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
					info.GetDamageName(),
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
					info.GetDamageName(),
					info.IsPlayer ? (" playing as " + info.RHub.characterClassManager.CurRole.fullName + ".") : "."
						}), flag2 ? ServerLogs.ServerLogType.Teamkill : ServerLogs.ServerLogType.KillLog, false);
					}

					if (info.GetDamageType().isScp || info.GetDamageType() == DamageTypes.Pocket)
						RoundSummary.kills_by_scp++;

					else if (info.GetDamageType() == DamageTypes.Grenade)
						RoundSummary.kills_by_frag++;

					try
					{
						Server.Get.Events.Player.InvokePlayerDeathEvent(victim, killer, info);
					}

					catch (Exception e)
					{
						SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDeath Event failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
					}

					if (!__instance._pocketCleanup || info.GetDamageType() != DamageTypes.Pocket)
					{
						referenceHub.inventory.ServerDropAll();
						PlayerMovementSync playerMovementSync = referenceHub.playerMovementSync;
						if (characterClassManager.Classes.CheckBounds(characterClassManager.CurClass) && info.GetDamageType() != DamageTypes.RagdollLess)
							__instance.GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, (playerMovementSync == null) ? Vector3.zero : playerMovementSync.PlayerVelocity, (int)characterClassManager.CurClass, info, characterClassManager.CurRole.team > Team.SCP, go.GetComponent<Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer>().PlayerId, referenceHub.nicknameSync.DisplayName, referenceHub.queryProcessor.PlayerId);
					}
					else
						referenceHub.inventory.Clear();

					characterClassManager.NetworkDeathPosition = go.transform.position;

					if (characterClassManager.CurRole.team == Team.SCP)
					{
						if (characterClassManager.CurClass == RoleType.Scp0492)
							NineTailedFoxAnnouncer.CheckForZombies(go);
						else
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
								DamageTypes.DamageType damageType = info.GetDamageType();
								if (damageType == DamageTypes.Tesla)
									NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "TESLA");
								else if (damageType == DamageTypes.Nuke)
									NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "WARHEAD");
								else if (damageType == DamageTypes.Decont)
									NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "DECONTAMINATION");
								else if (characterClassManager.CurClass != RoleType.Scp079)
									NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "UNKNOWN");
							}
						}
					}

					playerStats.SetHPAmount(100);
					characterClassManager.SetClassID(RoleType.Spectator);

					victim.CustomRole = null;
					foreach (var larry in Server.Get.Players.Where(x => x.Scp106Controller.PocketPlayers.Contains(victim)))
						larry.Scp106Controller.PocketPlayers.Remove(victim);
				}
				else
				{
					Vector3 pos = Vector3.zero;
					float num3 = 40f;
					if (info.GetDamageType().isWeapon)
					{
						GameObject playerOfID = __instance.GetPlayerOfID(info.PlayerId);
						if (playerOfID != null)
						{
							pos = go.transform.InverseTransformPoint(playerOfID.transform.position).normalized;
							num3 = 100f;
						}
					}
					else if (info.GetDamageType() == DamageTypes.Pocket)
					{
						PlayerMovementSync component2 = __instance.ccm.GetComponent<PlayerMovementSync>();
						if (component2.RealModelPosition.y > -1900f)
							component2.OverridePosition(Vector3.down * 1998.5f, 0f, true);
					}
					__instance.TargetBloodEffect(go.GetComponent<NetworkIdentity>().connectionToClient, pos, Mathf.Clamp01(info.Amount / num3));
				}
				Respawning.RespawnTickets singleton = Respawning.RespawnTickets.Singleton;
				Team team = characterClassManager.CurRole.team;
				byte b = (byte)team;
				if (b != 0)
				{
					if (b == 3)
					{
						if (flag)
						{
							Team team2 = __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team;
							if (team2 == Team.CDP && team2 == Team.CHI)
								singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, __instance._respawn_tickets_ci_scientist_died_count, false);
						}
					}
				}
				else if (characterClassManager.CurClass != RoleType.Scp0492)
					for (float num4 = 1f; num4 > 0f; num4 -= __instance._respawn_tickets_mtf_scp_hurt_interval)
					{
						float num5 = (float)playerStats.maxHP * num4;
						if (health > num5 && playerStats.Health < num5)
						{
							singleton.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_hurt_count, false);
						}
					}

				if (component != null && (object)component.CurrentScp is PlayableScps.Interfaces.IDamagable damagable2)
					damagable2.OnDamage(info);

				if (!flag4 || FriendlyFireConfig.PauseDetector || PermissionsHandler.IsPermitted(info.RHub.serverRoles.Permissions, PlayerPermissions.FriendlyFireDetectorImmunity))
				{
					__result = flag;
					return false;
				}

				if (FriendlyFireConfig.IgnoreClassDTeamkills && referenceHub.characterClassManager.CurRole.team == Team.CDP && info.RHub.characterClassManager.CurRole.team == Team.CDP)
				{
					__result = flag;
					return false;
				}

				if (flag2)
				{
					if (info.RHub.FriendlyFireHandler.Respawn.RegisterKill())
					{
						__result = flag;
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Window.RegisterKill())
					{
						__result = flag;
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Life.RegisterKill())
					{
						__result = flag;
						return false;
					}
					if (info.RHub.FriendlyFireHandler.Round.RegisterKill())
					{
						__result = flag;
						return false;
					}
				}

				if (info.RHub.FriendlyFireHandler.Respawn.RegisterDamage(info.Amount))
				{
					__result = flag;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Window.RegisterDamage(info.Amount))
				{
					__result = flag;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Life.RegisterDamage(info.Amount))
				{
					__result = flag;
					return false;
				}

				info.RHub.FriendlyFireHandler.Round.RegisterDamage(info.Amount);
				__result = flag;
				return false;
			}
			catch (Exception e)
			{
				SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage Patch failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
				__result = false;
				return true;
			}
		}
	}
}