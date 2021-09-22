using System;
using System.Linq;
using CustomPlayerEffects;
using Dissonance.Integrations.MirrorIgnorance;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using Respawning;
using Synapse.Api;
using UnityEngine;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    internal static class PlayerDamagePatch2
    {
        [HarmonyPrefix]
        private static bool HurtPlayer(PlayerStats __instance, out bool __result, PlayerStats.HitInfo info, GameObject go, bool noTeamDamage = false, bool IsValidDamage = true)
        {
            try
            {
                __result = false;
                var victim = go?.GetPlayer();
                var killer = __instance.GetPlayer();
                if (victim == null)
                    return false;
                if (victim != null && victim.Hub.characterClassManager.CurClass == RoleType.Spectator)
                    return false;

                bool died = false;
                float artificialHealth = default;
                float health = default;
                bool friendlyFire = default;
                DamageTypes.DamageType damageType = default;

                if (!TryPlayerDamageEvent(killer, victim, out (float artificialHealth, float health, bool friendlyFire, DamageTypes.DamageType damageType) temp))
                    return false;

                (artificialHealth, health, friendlyFire, damageType) = (temp.artificialHealth, temp.health, temp.friendlyFire, temp.damageType);

                HandleDamage(artificialHealth, victim);

                PlayableScpsController component = go.GetComponent<PlayableScpsController>();
                if (component != null && component.CurrentScp is PlayableScps.Interfaces.IDamagable damagable)
                    damagable.OnDamage(info);

                AddRespawnTickets(victim, health);

                ApplyRpcEffects(damageType);

                if (victim.Health < 1f)
                {
                    if (component != null && component.CurrentScp is PlayableScps.Interfaces.IImmortalScp immortalScp && !immortalScp.OnDeath(info, __instance.gameObject))
                        return false;

                    PlayerDeathEvent(victim, killer, friendlyFire, damageType, ref died);
                }
                else if (damageType == DamageTypes.Pocket)
                {
                    if (victim.Position.y > -1900f)
                        victim.Position = Vector3.down * 1995.5f;
                }

                __result = died;
                
                if (component != null && component.CurrentScp is PlayableScps.Interfaces.IDamagable damagable2)
                    damagable2.OnDamage(info);

                if (!friendlyFire || FriendlyFireConfig.PauseDetector || PermissionsHandler.IsPermitted(killer.ServerRoles.Permissions, PlayerPermissions.FriendlyFireDetectorImmunity))
                    return false;

                //This just blocks any TeamKillReport that is connected with CustomRoles
                if (victim.CustomRole != null || killer.CustomRole != null)
                    return false;

                if (friendlyFire)
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

            void PlayerDeathEvent(Player victim, Player killer, bool friendlyFire, DamageTypes.DamageType damageType, ref bool died)
            {
                //TODO:
                //foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
                //    scp079PlayerScript.ServerProcessKillAssist(victim.Hub, ExpGainType.KillAssist);

                HandleRoundStatsAndArchievements(victim, killer, friendlyFire, ref died);

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
                    victim.Inventory.DropAll();
                    var flag6 = __instance.TryGet096SmackVelocity(killer.Hub, victim.Hub, out Vector3 vector);
                    var vector2 = flag6 ? vector : victim.PlayerMovementSync.PlayerVelocity;
                    if (victim.ClassManager.Classes.CheckBounds(victim.RoleType) && damageType != DamageTypes.RagdollLess)
                    {
                        __instance.GetComponent<RagdollManager>().SpawnRagdoll(
                            go.transform.position, go.transform.rotation, vector2,
                            (int)victim.RoleType, info, victim.Team > Team.SCP,
                            go.GetComponent<MirrorIgnorancePlayer>().PlayerId,
                            victim.DisplayName, victim.PlayerId, flag6);
                    }
                }
                victim.ClassManager.DeathPosition = go.transform.position;

                AnnounceScpDeath(victim, damageType);

                victim.PlayerStats.SetHPAmount(100);
                victim.ClassManager.SetClassID(RoleType.Spectator, CharacterClassManager.SpawnReason.Died);

                victim.CustomRole = null;
                foreach (var larry in Server.Get.Players.Where(x => x.Scp106Controller.PocketPlayers.Contains(victim)))
                    larry.Scp106Controller.PocketPlayers.Remove(victim);

                if (victim.IsDummy)
                    Map.Get.Dummies.FirstOrDefault(x => x.Player == victim)?.Destroy();
            }
            void AnnounceScpDeath(Player victim, DamageTypes.DamageType damageType)
            {
                if (victim.Team == Team.SCP && victim.RoleType != RoleType.Scp0492)
                {
                    GameObject playerGameObject = PlayerManager.players.FirstOrDefault(_ => _.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId == info.PlayerId);

                    if (playerGameObject != null)
                    {
                        NineTailedFoxAnnouncer.AnnounceScpTermination(victim.ClassManager.CurRole, info, string.Empty);
                    }
                    else if (damageType == DamageTypes.Tesla)
                    {
                        NineTailedFoxAnnouncer.AnnounceScpTermination(victim.ClassManager.CurRole, info, "TESLA");
                    }
                    else if (damageType == DamageTypes.Nuke)
                    {
                        NineTailedFoxAnnouncer.AnnounceScpTermination(victim.ClassManager.CurRole, info, "WARHEAD");
                    }
                    else if (damageType == DamageTypes.Decont)
                    {
                        NineTailedFoxAnnouncer.AnnounceScpTermination(victim.ClassManager.CurRole, info, "DECONTAMINATION");
                    }
                    else if (victim.RoleType != RoleType.Scp079)
                    {
                        NineTailedFoxAnnouncer.AnnounceScpTermination(victim.ClassManager.CurRole, info, "UNKNOWN");
                    }
                }
            }
            void HandleRoundStatsAndArchievements(Player victim, Player killer, bool friendlyFire, ref bool died)
            {
                if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60 && IsValidDamage)
                    __instance.TargetAchieve(victim.Connection, "wowreally");

                if (__instance.isLocalPlayer && info.PlayerId != victim.PlayerId)
                    RoundSummary.Kills++;

                died = true;

                if (info.RHub != null && info.RHub.Ready && IsValidDamage)
                    GameplayTickets.Singleton.TicketBasedKilling(victim.Team, killer.Team);

                if (victim.RoleType == RoleType.Scp096)
                {
                    if (victim.Hub.scpsController.CurrentScp is PlayableScps.Scp096 scp096 && scp096.PlayerState == PlayableScps.Scp096PlayerState.Enraging)
                        __instance.TargetAchieve(victim.Connection, "unvoluntaryragequit");
                }
                else if (info.Tool == DamageTypes.Pocket)
                {
                    __instance.TargetAchieve(victim.Connection, "newb");
                }
                else if (info.Tool == DamageTypes.Scp173)
                {
                    __instance.TargetAchieve(victim.Connection, "firsttime");
                }
                else if (info.Tool == DamageTypes.Grenade && info.PlayerId == victim.PlayerId)
                {
                    __instance.TargetAchieve(victim.Connection, "iwanttobearocket");
                }
                else if (info.Tool.Weapon != ItemType.None)
                {
                    if (victim.RoleType == RoleType.Scientist && victim.ItemInHand.ID != -1 &&
                        victim.ItemInHand.ItemCategory == ItemCategory.Keycard &&
                        killer.RoleType == RoleType.ClassD)
                    {
                        __instance.TargetAchieve(__instance.connectionToClient, "betrayal");
                    }

                    if (Time.realtimeSinceStartup - __instance._killStreakTime > 30f || __instance._killStreak == 0)
                    {
                        __instance._killStreak = 0;
                        __instance._killStreakTime = Time.realtimeSinceStartup;
                    }

                    if (HitboxIdentity.CheckFriendlyFire(killer.Hub, victim.Hub, false))
                        __instance._killStreak++;

                    if (__instance._killStreak >= 5)
                        __instance.TargetAchieve(__instance.connectionToClient, "pewpew");

                    if ((killer.Team == Team.MTF || killer.Team == Team.RSC) && victim.RoleType == RoleType.ClassD)
                        __instance.TargetStats(killer.Connection, "dboys_killed", "justresources", 50);
                }
                else if (victim.Team == Team.SCP && victim.ItemInHand.ID != -1 && victim.ItemInHand.ItemBase is MicroHIDItem microHIDItem && microHIDItem.State != HidState.Idle)
                {
                    __instance.TargetAchieve(__instance.connectionToClient, "illpassthanks");
                }

                if (victim.Team == Team.RSC && died && info.RHub != null)
                {
                    if (victim.Team == Team.CDP || victim.Team == Team.CHI)
                        RespawnTickets.Singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, __instance._respawn_tickets_ci_scientist_died_count, false);
                }
                

                if (victim.RealTeam == Team.RSC && victim.RealTeam == Team.SCP)
                    __instance.TargetAchieve(__instance.connectionToClient, "timetodoitmyself");

                ConsoleLogging(victim, killer, friendlyFire);

                

                if (info.Tool.Scp != RoleType.None || info.Tool == DamageTypes.Pocket)
                    RoundSummary.kills_by_scp++;

                else if (info.Tool == DamageTypes.Grenade)
                    RoundSummary.kills_by_frag++;

            }
            void ConsoleLogging(Player victim, Player killer, bool friendlyFire)
            {
                bool suicide = victim == killer;

                if (suicide)
                {
                    ServerLogs.AddLog(
                        ServerLogs.Modules.ClassChange,
                        $"{victim.Hub.LoggedNameFromRefHub()} playing as {victim.ClassManager.CurRole.fullName} committed a suicide using {info.Tool.Name}.",
                        ServerLogs.ServerLogType.Suicide,
                        false
                        );
                }
                else
                {
                    ServerLogs.AddLog(
                        ServerLogs.Modules.ClassChange,
                        $"{victim.Hub.LoggedNameFromRefHub()} playing as {victim.ClassManager.CurRole.fullName} has been killed by {killer.NickName} using {info.Tool.Name}" +
                        $"{(info.IsPlayer ? (" playing as " + info.RHub.characterClassManager.CurRole.fullName + ".") : ".")}",
                        friendlyFire ? ServerLogs.ServerLogType.Teamkill : ServerLogs.ServerLogType.KillLog,
                        false
                        );

                    if (info.Tool == DamageTypes.Lure)
                    {
                        victim.ClassManager.TargetConsolePrint(victim.Connection, "You sacrificed yourself for the femur breaker.", "yellow");
                    }
                    else if (info.Tool == DamageTypes.Recontainment)
                    {
                        victim.ClassManager.TargetConsolePrint(victim.Connection, "You have been recontained.", "yellow");
                    }
                    else
                    {
                        victim.ClassManager.TargetConsolePrint(
                            victim.Connection,
                            $"{info.ClientAttackerName} {(info.IsPlayer ? ("playing as " + info.RHub.characterClassManager.CurRole.fullName) : "")} killed you with {info.Tool.Name}",
                            "yellow"
                            );
                    }
                }
            }
            void ApplyRpcEffects(DamageTypes.DamageType damageType)
            {
                byte damageTypeId = (byte)DamageTypes.ToIndex(damageType);
                foreach (var value in __instance.Hub.playerEffectsController.AllEffects.Values)
                {
                    if (value.IsEnabled && value is IOnDamageTakenEffect effect2 && effect2.AllowPulse(damageType))
                    {
                        __instance.RpcTriggerPostProcessing(damageTypeId);
                    }
                }
            }
            void AddRespawnTickets(Player victim, float health)
            {
                RespawnTickets singleton = RespawnTickets.Singleton;
                if (victim.Team == Team.SCP && victim.RoleType != RoleType.Scp0492)
                {
                    if (victim.RoleType != RoleType.Scp079)
                    {
                        for (float num4 = 1f; num4 > 0f; num4 -= __instance._respawn_tickets_mtf_scp_hurt_interval)
                        {
                            float num5 = victim.MaxHealth * num4;
                            if (health > num5 && victim.Health < num5)
                                singleton.GrantTickets(SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_hurt_count, false);
                        }
                    }
                    if (victim.Health < 1f)
                        singleton.GrantTickets(SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_death_count, false);
                }
            }
            void HandleDamage(float artificialHealth, Player victim)
            {
                if (artificialHealth > 0f)
                {
                    float num2 = info.Amount * victim.PlayerStats.ArtificialNormalRatio;
                    float num3 = info.Amount - num2;
                    artificialHealth -= num2;
                    if (artificialHealth < 0f)
                    {
                        num3 += Mathf.Abs(artificialHealth);
                    }
                    victim.ArtificialHealth = artificialHealth;
                    victim.Health -= num3;

                    if (victim.Health > 0f && victim.Health - num2 <= 0f && victim.Team != Team.SCP)
                        __instance.TargetAchieve(victim.Connection, "didntevenfeelthat");
                }
                else
                {
                    victim.Health -= info.Amount;
                }

                if (victim.Health < 0f)
                    victim.Health = 0f;

                victim.PlayerStats.lastHitInfo = info;

            }
            bool TryPlayerDamageEvent(Player killer, Player victim, out (float ArtificialHealth, float Health, bool FriendlyFire, DamageTypes.DamageType DamageType) paramInfo)
            {
                paramInfo.ArtificialHealth = 0;
                paramInfo.Health = 0;
                paramInfo.DamageType = null;
                paramInfo.FriendlyFire = false;
                
                if (info.Tool == DamageTypes.Grenade)
                {
                    killer = SynapseController.Server.GetPlayer(info.PlayerId);
                }
                else if (info.Tool == DamageTypes.Pocket)
                {
                    killer = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(victim));

                    if (!SynapseExtensions.CanHarmScp(victim, false))
                        return false;
                }
                

                if (killer == null || killer.Hub.isDedicatedServer)
                    killer = victim;

                paramInfo.DamageType = info.Tool;

                
                if (victim.RoleType == RoleType.Spectator)
                    return false;

                if (info.Amount < 0f)
                    info.Amount = Mathf.Abs(victim.Health + victim.ArtificialHealth + 10f);

                var effect = victim.PlayerEffectsController.GetEffect<CustomPlayerEffects.Burned>();
                if (effect != null && effect.IsEnabled)
                    info.Amount += effect.damageMultiplier;
                

                if (info.Amount > 2.14748365E+09f)
                    info.Amount = 2.14748365E+09f;

                if (victim.GodMode)
                    return false;

                if (victim != killer && victim.Team == Team.SCP && killer.Team == Team.SCP) return false;

                if (victim.ClassManager.SpawnProtected && !killer.PlayerStats._allowSPDmg)
                    return false;
                
                paramInfo.FriendlyFire = !noTeamDamage && info.IsPlayer && victim != killer && victim.Faction == killer.Faction;
                if (paramInfo.FriendlyFire)
                    info.Amount += PlayerStats.FriendlyFireFactor;

                paramInfo.Health = victim.Health;
                paramInfo.ArtificialHealth = victim.ArtificialHealth;
                
                try
                {
                    Server.Get.Events.Player.InvokePlayerDamageEvent(victim, killer, ref info, out var allow);
                    if (!allow) return false;
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Event: PlayerDamage Event failed!!\n{e}");
                }
                
                return true;
            }
        }
    }
}