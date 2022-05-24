using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using System;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBulletholeDecal))]
    internal static class PlayerPlaceBulletHolePatch
    {
        [HarmonyPrefix]
        private static bool PlaceBulletHole(StandardHitregBase __instance, RaycastHit hit)
        {
            try
            {
                var player = __instance?.Hub?.GetPlayer();
                if (player is null)
                    return false;
                var point = hit.point;
                var normal = hit.normal;

                Server.Get.Events.Player.InvokePlaceBulletHoleEvent(player, point, out var allow);

                return allow;
            }
            catch (Exception ex)
            {
                Server.Get.Logger.Error("Synapse-Event: PlaceBulletHoleEvent failed!!\n" + ex);
                return true;
            }
        }
    }
}
