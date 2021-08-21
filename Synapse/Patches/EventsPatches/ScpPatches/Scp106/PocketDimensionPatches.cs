using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
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
        private static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || ply == null) return false;
                var scp = __instance.GetPlayer();
                var player = ply.GetPlayer();
                if (player == null || player.GodMode || !ServerTime.CheckSynchronization(t) || !player.ClassManager.IsHuman()) 
                    return false;

                if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, player.Hub))
                    return false;

                var pos = player.Position;
                var num = Vector3.Distance(scp.Position, pos);
                var num2 = Math.Abs(scp.Position.y - pos.y);
                if ((num >= 1.818f && num2 < 1.02f) || (num >= 2.1f && num2 < 1.95f) || (num >= 2.65f && num2 < 2.2f) || (num >= 3.2f && num2 < 3f) || num >= 3.64f)
                {
                    __instance._hub.characterClassManager.TargetConsolePrint(scp.Connection, string.Format("106 MovePlayer command rejected - too big distance (code: T1). Distance: {0}, Y Diff: {1}.", num, num2), "gray");
                    return false;
                }
                if (Physics.Linecast(scp.Position, ply.transform.position, MicroHIDItem.WallMask))
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

                    if (player.RoleType == RoleType.Spectator) return false;

                    EventHandler.Get.Scp.Scp106.InvokePocketDimensionEnterEvent(player, scp, ref allow);
                    if (!allow) return false;
                    scp.Scp106Controller.PocketPlayers.Add(player);
                    player.GiveEffect(Api.Enum.Effect.Corroding,0);
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
        private static bool Prefix(PocketDimensionTeleport __instance, Collider other)
        {
            try
            {
                if (!NetworkServer.active) return false;

                var component = other.GetComponent<NetworkIdentity>();
                if (component == null) return false;

                var type = __instance.type;
                var player = component.GetPlayer();

                if (player == null) return false;

                __instance.tpPositions.Clear();
                var stringList = GameCore.ConfigFile.ServerConfig.GetStringList(Synapse.Api.Map.Get.Decontamination.IsDecontaminationInProgress ? "pd_random_exit_rids_after_decontamination" : "pd_random_exit_rids");
                foreach (GameObject gameObject in Server.Get.Map.Rooms.Select(x => x.GameObject))
                {
                    var component2 = gameObject.GetComponent<Rid>();
                    if (component2 != null && stringList.Contains(component2.id, StringComparison.Ordinal))
                        __instance.tpPositions.Add(gameObject.transform.position);
                }

                if (stringList.Contains("PORTAL"))
                    foreach (Scp106PlayerScript scp106PlayerScript in UnityEngine.Object.FindObjectsOfType<Scp106PlayerScript>())
                        if (scp106PlayerScript.portalPosition != Vector3.zero)
                            __instance.tpPositions.Add(scp106PlayerScript.portalPosition);

                if(__instance.tpPositions == null || __instance.tpPositions.Count == 0)
                    foreach (GameObject gameObject2 in GameObject.FindGameObjectsWithTag("PD_EXIT"))
                        __instance.tpPositions.Add(gameObject2.transform.position);

                var pos = __instance.tpPositions[UnityEngine.Random.Range(0, __instance.tpPositions.Count)];
                pos.y += 2f;

                if (!SynapseExtensions.CanHarmScp(player, false) || player.Team == Team.SCP)
                    type = PocketDimensionTeleport.PDTeleportType.Exit;

                EventHandler.Get.Scp.Scp106.InvokePocketDimensionLeaveEvent(player, ref pos, ref type, out var allow);

                if (!allow) return false;

                if (type == PocketDimensionTeleport.PDTeleportType.Killer || BlastDoor.OneDoor.isClosed)
                {
                    player.Hurt(9999, DamageTypes.Pocket, player);
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
                Logger.Get.Error($"Synapse-Event: PocketDimLeave failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}