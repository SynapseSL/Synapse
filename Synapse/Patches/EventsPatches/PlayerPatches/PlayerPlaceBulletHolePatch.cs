using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using Synapse.Api;
using System;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBloodDecal))]
    internal static class PlayerPlaceBulletHolePatch
    {
        [HarmonyPrefix]
        private static bool PlaceBulletHole(StandardHitregBase __instance, Ray ray, RaycastHit hit)
        {
            try
            {
                var player = __instance.Hub.GetPlayer();
                var point = hit.point;
                var normal = hit.normal;
                
                Server.Get.Events.Player.InvokePlaceBulletHoleEvent(player, ref point, ref normal, out var allow);
                
                if (!allow)
                    return false;

                hit.point = point;
                hit.normal = normal;
                return true;
            }
            catch (Exception ex)
            {
                Server.Get.Logger.Error(string.Format("Synapse-Event: PlaceBulletHoleEvent failed!!\n{0}", (object)ex));
                return true;
            }
        }
    }
}
