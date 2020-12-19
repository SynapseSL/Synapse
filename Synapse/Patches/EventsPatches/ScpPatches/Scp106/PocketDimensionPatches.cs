using System;
using System.Linq;
using HarmonyLib;
using Mirror;
using UnityEngine;
using EventHandler = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp106
{
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
    internal class PocketDimensionEnterPatch
    {
        private static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || ply == null) return false;
                var scp = __instance.GetPlayer();
                var player = ply.GetPlayer();
                if (player == null || player.GodMode || !ServerTime.CheckSynchronization(t) || !__instance.iAm106 || Vector3.Distance(scp.Position,player.Position) >= 3f) 
                    return false;

                if (!scp.WeaponManager.GetShootPermission(player.ClassManager))
                    return false;

                EventHandler.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp106_Grab, out var allow);
                if (!allow) return false;

                scp.ClassManager.RpcPlaceBlood(player.Position, 1, 2f);
                __instance.TargetHitMarker(scp.Connection);
                if (Scp106PlayerScript._blastDoor.isClosed)
                    player.Hurt(500, DamageTypes.Scp106, scp);
                else
                {
                    player.Hurt(40, DamageTypes.Scp106, scp);
                    player.Position = Vector3.down * 1998.5f;
                    foreach(var scp079 in Scp079PlayerScript.instances)
                    {
                        var room = player.ClassManager.Scp079.GetOtherRoom();
                        var filter = new Scp079Interactable.InteractableType[]
                        {
                            Scp079Interactable.InteractableType.Door,
                            Scp079Interactable.InteractableType.Light,
                            Scp079Interactable.InteractableType.Lockdown,
                            Scp079Interactable.InteractableType.Tesla,
                            Scp079Interactable.InteractableType.ElevatorUse,
                        };

                        var flag = true;
                        foreach (var interaction in scp079.ReturnRecentHistory(12f, filter))
                            foreach (var zoneRoom in interaction.interactable.currentZonesAndRooms)
                                if (zoneRoom.currentZone == room.currentZone && zoneRoom.currentRoom == room.currentRoom)
                                    flag = true;
                        if (flag)
                            scp079.RpcGainExp(ExpGainType.PocketAssist, player.RoleType);
                    }

                    if (player.RoleType == RoleType.Spectator) return false;

                    EventHandler.Get.Scp.Scp106.InvokePocketDimensionEnterEvent(player, scp, ref allow);
                    if (!allow) return false;
                    scp.Scp106Controller.PocketPlayers.Add(player);
                    player.PlayerEffectsController.GetEffect<CustomPlayerEffects.Corroding>().IsInPd = true;
                    player.GiveEffect(Api.Enum.Effect.Corroding);
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PocketDimEnter/ScpAttackEvent(106) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
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

                if ((player.CustomRole != null && player.CustomRole.GetFriends().Any(x => x == Team.SCP)) || player.Team == Team.SCP)
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
                    ImageGenerator.pocketDimensionGenerator.GenerateRandom();
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