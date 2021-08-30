using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using LightContainmentZoneDecontamination;
using MapGeneration;
using Mirror;
using UnityEngine;
using EventHandler = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp106
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.UserCode_CmdMovePlayer))]
    internal class PocketDimensionEnterPatch
    {
        [HarmonyPrefix]
        private static bool MovePlayer(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || ply == null || !__instance.iAm106) return false;

                var scp = __instance.GetPlayer();
                var player = ply.GetPlayer();

                if (player == null || player.GodMode || !ServerTime.CheckSynchronization(t) || !player.ClassManager.IsHuman()) 
                    return false;

                if (!SynapseExtensions.GetHarmPermission(scp, player))
                    return false;

                var pos = player.Position;
                var num = Vector3.Distance(scp.Position, pos);
                var num2 = Math.Abs(scp.Position.y - pos.y);
                if ((num >= 1.818f && num2 < 1.02f) || (num >= 3.4f && num2 < 1.95f) || (num >= 3.7f && num2 < 2.2f) || (num >= 3.9f && num2 < 3f) || num >= 4.2f)
                {
                    __instance._hub.characterClassManager.TargetConsolePrint(scp.Connection, string.Format("106 MovePlayer command rejected - too big distance (code: T1). Distance: {0}, Y Diff: {1}.", num, num2), "gray");
                    return false;
                }
                if (Physics.Linecast(scp.Position, player.Position, MicroHIDItem.WallMask))
                {
                    __instance._hub.characterClassManager.TargetConsolePrint(scp.Connection, string.Format("106 MovePlayer command rejected - collider found between you and the target (code: T2). Distance: {0}, Y Diff: {1}.", num, num2), "gray");
                    return false;
                }

                EventHandler.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp106_Grab, out var allow);
                if (!allow) return false;

                scp.ClassManager.RpcPlaceBlood(player.Position, 1, 2f);
                __instance.TargetHitMarker(scp.Connection, __instance.captureCooldown);
                __instance._currentServerCooldown = __instance.captureCooldown;
                if (Scp106PlayerScript._blastDoor.isClosed)
                {
                    __instance._hub.characterClassManager.RpcPlaceBlood(player.Position, 1, 2f);
                    player.Hurt(500, DamageTypes.Scp106, scp);
                }
                else
                {
                    player.Hurt(40, DamageTypes.Scp106, scp);
                    player.Position = Vector3.down * 1998.5f;
                    foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
                    {
                        Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
                        {
                    Scp079Interactable.InteractableType.Door,
                    Scp079Interactable.InteractableType.Light,
                    Scp079Interactable.InteractableType.Lockdown,
                    Scp079Interactable.InteractableType.Tesla,
                    Scp079Interactable.InteractableType.ElevatorUse
                        };
                        bool flag = false;
                        using (IEnumerator<Scp079Interaction> enumerator2 = scp079PlayerScript.ReturnRecentHistory(12f, filter).GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (RoomIdUtils.IsTheSameRoom(enumerator2.Current.interactable.transform.position, ply.transform.position))
                                {
                                    flag = true;
                                }
                            }
                        }
                        if (flag)
                        {
                            scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, player.RoleType);
                        }
                    }

                    EventHandler.Get.Scp.Scp106.InvokePocketDimensionEnterEvent(player, scp, ref allow);
                    if (!allow) return false;
                    scp.Scp106Controller.PocketPlayers.Add(player);

                    player.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Corroding>(0f, false);
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PocketDimEnter/ScpAttackEvent(106) failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    internal class PocketDimensionLeavePatch
    {
        [HarmonyPrefix]
        private static bool TriggerEnter(PocketDimensionTeleport __instance, Collider other)
        {
            try
            {
                var component = other.GetComponent<NetworkIdentity>();
                if (component == null) return false;

                var type = __instance.type;
                var player = component.GetPlayer();
                if (player == null) return false;

                var forceEscape = !SynapseExtensions.CanHarmScp(player, false);

                __instance.tpPositions.Clear();
                var phases = DecontaminationController.Singleton.DecontaminationPhases;
                var flag = DecontaminationController.GetServerTime > phases[phases.Length - 2].TimeTrigger;

                var list = GameCore.ConfigFile.ServerConfig.GetStringList(flag ? "pd_random_exit_rids_after_decontamination" : "pd_random_exit_rids");
                if(list.Count > 0)
                    foreach (var gameObject in GameObject.FindGameObjectsWithTag("RoomID"))
                    {
                        var rid = gameObject.GetComponent<Rid>();
                        if (rid != null && list.Contains(rid.id))
                            __instance.tpPositions.Add(gameObject.transform.position);
                    }
                if (list.Contains("PORTAL"))
                    foreach (var scp106 in UnityEngine.Object.FindObjectsOfType<Scp106PlayerScript>())
                        if (scp106.portalPosition != Vector3.zero)
                            __instance.tpPositions.Add(scp106.portalPosition);

                if(__instance.tpPositions == null || __instance.tpPositions.Count == 0)
                    foreach (var defaultexits in GameObject.FindGameObjectsWithTag("PD_EXIT"))
                        __instance.tpPositions.Add(defaultexits.transform.position);

                var pos = __instance.tpPositions[UnityEngine.Random.Range(0, __instance.tpPositions.Count)];
                pos.y += 2f;

                if (Synapse.Api.Nuke.Get.Detonated)
                    pos = new Vector3(187f, 1000f, -85f);


                EventHandler.Get.Scp.Scp106.InvokePocketDimensionLeaveEvent(player, ref pos, ref type, out var allow);

                if (!allow) return false;

                if(!forceEscape && (type == PocketDimensionTeleport.PDTeleportType.Killer || Synapse.Api.Nuke.Get.Detonated))
                {
                    player.Hurt(999999, DamageTypes.Pocket);
                    return false;
                }

                player.PlayerMovementSync.AddSafeTime(2f, false);
                player.Position = pos;
                __instance.RemoveCorrosionEffect(player.gameObject);
                PlayerManager.localPlayer.GetComponent<PlayerStats>().TargetAchieve(component.connectionToClient, "larryisyourfriend");

                if (PocketDimensionTeleport.RefreshExit)
                    MapGeneration.ImageGenerator.pocketDimensionGenerator.GenerateRandom();

                return false;
            } 
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PocketDimLeave failed!!\n{e}");
                return true;
            }
        }
    }
}