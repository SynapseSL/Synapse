using System;
using Harmony;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.Item_Patches
{
    [HarmonyPatch(typeof(WorkStation),nameof(WorkStation.ConnectTablet))]
    internal static class WorkStationPatch
    {
        private static bool Prefix(WorkStation __instance, GameObject tabletOwner)
        {
            if (!__instance.CanPlace(tabletOwner) || __instance._animationCooldown > 0f) return false;

            var player = tabletOwner.GetPlayer();
            var station = __instance.GetWorkStation();

            foreach(var item in player.Inventory.Items)
                if(item.ItemType == ItemType.WeaponManagerTablet && !item.IsCustomItem)
                {
                    item.Despawn();
                    station.ConnectedTablet = item;
                    station.TabletOwner = tabletOwner.GetPlayer();
                }
            return false;
        }
    }

    [HarmonyPatch(typeof(WorkStation), nameof(WorkStation.UnconnectTablet))]
    internal static class WorkStationPatch2
    {
        private static bool Prefix(WorkStation __instance, GameObject taker)
        {
            try
            {
                if (!__instance.CanTake(taker) || __instance._animationCooldown > 0f) return false;

                var player = taker.GetPlayer();
                var station = __instance.GetWorkStation();

                

                station.TabletOwner = player;
                station.IsTabletConnected = false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: Unconnect Workstation Tablet failed!!\n{e}");
            }
            return false;
        }
    }
}
