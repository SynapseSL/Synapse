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
                var allow = true;
                var larry = __instance.GetPlayer();
                var player = ply.GetPlayer();
                EventHandler.Get.Scp.Scp106.InvokePocketDimensionEnterEvent(player,larry, ref allow);

                if (allow)
                    larry.Scp106Controller.PocketPlayers.Add(player);

                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PocketDimEnter failed!!\n{e}");
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
                    var component2 = gameObject.GetComponent<global::Rid>();
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
                Logger.Get.Error($"Synapse-Event: PocketDimLeave failed!!\n{e}");
                return true;
            }
        }
    }
}