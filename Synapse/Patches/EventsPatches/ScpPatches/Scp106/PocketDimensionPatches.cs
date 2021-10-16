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

                var type = __instance._type;
                var player = component.GetPlayer();
                var pos = Vector3.zero;
                if (player == null) return false;

                var forceEscape = !SynapseExtensions.CanHarmScp(player, false);

                if (player.Hub.scp106PlayerScript.GrabbedPosition == Vector3.zero)
                    player.Hub.scp106PlayerScript.GrabbedPosition = new Vector3(0f, -1997f, 0f);

                var identifier = MapGeneration.RoomIdUtils.RoomAtPosition(player.Hub.scp106PlayerScript.GrabbedPosition);

                if(identifier.Zone == FacilityZone.Surface)
                {
                    foreach(var player2 in Server.Get.Players)
                        if(player2.RoleType == RoleType.Scp106)
                        {
                            var num = Vector3.Distance(player2.Position, __instance._gateBPDPosition);
                            var num2 = Vector3.Distance(player2.Position,__instance._gateBPDPosition);
                            pos = num2 < num ? __instance._gateBPDPosition : __instance._gateAPDPosition;
                            break;
                        }
                }
                else
                {
                    var hashSet = MapGeneration.RoomIdUtils.FindRooms(MapGeneration.RoomName.Unnamed, identifier.Zone, MapGeneration.RoomShape.Undefined);
                    hashSet.RemoveWhere((MapGeneration.RoomIdentifier room) => 
                    room.Name == MapGeneration.RoomName.Hcz106 || room.Name == MapGeneration.RoomName.EzGateA || 
                    room.Name == MapGeneration.RoomName.EzGateB || (room.Zone == MapGeneration.FacilityZone.LightContainment 
                    && room.Shape == MapGeneration.RoomShape.Curve) || __instance.ProblemChildren.Contains(room.Name));

                    while (hashSet.Count > 0)
                    {
                        MapGeneration.RoomIdentifier roomIdentifier2 = hashSet.ElementAt(UnityEngine.Random.Range(0, hashSet.Count));
                        if (PlayerMovementSync.FindSafePosition(roomIdentifier2.transform.position, out pos, false, true))
                            break;
                        hashSet.Remove(roomIdentifier2);
                    }
                }

                EventHandler.Get.Scp.Scp106.InvokePocketDimensionLeaveEvent(player, ref pos, ref type, out var allow);

                if (!allow) return false;

                if(!forceEscape && (type == PocketDimensionTeleport.PDTeleportType.Killer || Synapse.Api.Nuke.Get.Detonated))
                {
                    player.Hurt(999999, DamageTypes.Pocket);
                    return false;
                }
                else
                {
                    player.Position = pos;
                    player.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Disabled>(10f, true);
                    player.PlayerEffectsController.GetEffect<CustomPlayerEffects.Corroding>().Intensity = 0;
                    player.PlayerStats.TargetAchieve(component.connectionToClient, "larryisyourfriend");
                }
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