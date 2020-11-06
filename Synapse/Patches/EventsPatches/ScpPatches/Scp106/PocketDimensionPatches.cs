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
                var component = other.GetComponent<NetworkIdentity>();
                if (!NetworkServer.active || component == null) return false;
                
                var allow = true;
                EventHandler.Get.Scp.Scp106.InvokePocketDimensionLeaveEvent(component.GetPlayer(), ref __instance.type, ref allow);
                if(__instance.type == PocketDimensionTeleport.PDTeleportType.Exit)
                {
                    var larry = Server.Get.Players.FirstOrDefault(x => x.Scp106Controller.PocketPlayers.Contains(component.GetPlayer()));
                    if (larry != null)
                        larry.Scp106Controller.PocketPlayers.Remove(component.GetPlayer());
                }
                return allow;
            } 
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PocketDimLeave failed!!\n{e}");
                return true;
            }
        }
    }
}