using Achievements;
using HarmonyLib;
using InventorySystem.Items.MicroHID;
using MapGeneration;
using Mirror;
using PlayerStatsSystem;
using System;
using System.Linq;
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
                if (!__instance._iawRateLimit.CanExecute(true) || ply is null || !__instance.iAm106) return false;

                var scp = __instance.GetPlayer();
                var player = ply.GetPlayer();

                if (player is null || player.GodMode || !ServerTime.CheckSynchronization(t) || !player.ClassManager.IsHuman()) 
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
                    player.PlayerStats.DealDamage(new ScpDamageHandler(__instance._hub, PlayerStatsSystem.DeathTranslations.PocketDecay));
                }
                else
                {
                    EventHandler.Get.Scp.Scp106.InvokePocketDimensionEnterEvent(player, scp, ref allow);
                    if (!allow) return false;

                    foreach (var script in Scp079PlayerScript.instances)
                        script.ServerProcessKillAssist(player.Hub, ExpGainType.PocketAssist);

                    player.Hub.scp106PlayerScript.GrabbedPosition = player.Hub.playerMovementSync.RealModelPosition;
                    player.PlayerStats.DealDamage(new ScpDamageHandler(__instance._hub, 40f, PlayerStatsSystem.DeathTranslations.PocketDecay));
                    player.Position = Vector3.down * 1998.5f;
                    scp.Scp106Controller.PocketPlayers.Add(player);
                }
                player.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Corroding>(0f, false);

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
                if (component is null) return false;

                var type = __instance._type;
                var player = component.GetPlayer();
                var pos = Vector3.zero;
                if (player is null) return false;

                var forceEscape = !SynapseExtensions.CanHarmScp(player, false);
                if (player.Hub.scp106PlayerScript.GrabbedPosition == Vector3.zero)
                    player.Hub.scp106PlayerScript.GrabbedPosition = new Vector3(0f, -1997f, 0f);

                var identifier = MapGeneration.RoomIdUtils.RoomAtPosition(player.Hub.scp106PlayerScript.GrabbedPosition);
                if (identifier.Zone == FacilityZone.Surface)
                {
                    foreach(var player2 in Server.Get.Players)
                        if(player2.RoleType == RoleType.Scp106)
                        {
                            Vector3 objPos = (player2 is null)
                                ? Vector3.zero
                                : player2.PlayerMovementSync.RealModelPosition;
                            SafeTeleportPosition componentInChildren = identifier.GetComponentInChildren<SafeTeleportPosition>();
                            float num = Vector3.Distance(objPos, componentInChildren.SafePositions[0].position);
                            float num2 = Vector3.Distance(objPos, componentInChildren.SafePositions[1].position);
                            pos = (num2 < num) ? componentInChildren.SafePositions[0].position : componentInChildren.SafePositions[1].position;
                            break;
                        }
                }
                else
                {
                    var hashSet = MapGeneration.RoomIdUtils.FindRooms(MapGeneration.RoomName.Unnamed, identifier.Zone, MapGeneration.RoomShape.Undefined);
                    /*hashSet.RemoveWhere((MapGeneration.RoomIdentifier room) => 
                    room.Name == MapGeneration.RoomName.Hcz106 || room.Name == MapGeneration.RoomName.EzGateA || 
                    room.Name == MapGeneration.RoomName.EzGateB || room.Name == MapGeneration.RoomName.EzEvacShelter ||
                    (room.Zone == MapGeneration.FacilityZone.LightContainment && room.Shape == MapGeneration.RoomShape.Curve) ||
                    room.Zone == MapGeneration.FacilityZone.None || room.Name == MapGeneration.RoomName.Pocket || 
                    room.Name == MapGeneration.RoomName.HczTesla);*/

                    try
                    {
                        while (hashSet.Count > 0)
                        {
                            MapGeneration.RoomIdentifier roomIdentifier2 = hashSet.ElementAt(UnityEngine.Random.Range(0, hashSet.Count));
                            var safepos = roomIdentifier2.transform.position;
                            var safeTeleport = roomIdentifier2.GetComponentInChildren<SafeTeleportPosition>();
                            if (safeTeleport != null && safeTeleport.SafePositions?.Length != 0)
                                safepos = safeTeleport.SafePositions[UnityEngine.Random.Range(0, safeTeleport.SafePositions.Length)].position;

                            if (PlayerMovementSync.FindSafePosition(safepos, out pos, false, true))
                                break;
                            hashSet.Remove(roomIdentifier2);
                        }
                    }
                    catch(Exception ex)
                    {
                        //I don't know how but for some Reason this fails sometimes and the method is called a second time
                        //Logger.Get.Debug(ex);
                        return false;
                    }
                }
                EventHandler.Get.Scp.Scp106.InvokePocketDimensionLeaveEvent(player, ref pos, ref type, out var allow);
                
                if (!allow) return false;

                if (!forceEscape && (type == PocketDimensionTeleport.PDTeleportType.Killer || Synapse.Api.Nuke.Get.Detonated))
                {
                    player.PlayerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.PocketDecay));
                    return false;
                }
                else
                {
                    player.Position = pos;
                    player.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Disabled>(10f, true);
                    player.PlayerEffectsController.GetEffect<CustomPlayerEffects.Corroding>().Intensity = 0;
                    Achievements.AchievementHandlerBase.ServerAchieve(component.connectionToClient, AchievementName.LarryFriend);
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