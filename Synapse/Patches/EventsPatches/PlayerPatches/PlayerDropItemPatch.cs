using System;
using Harmony;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CallCmdDropItem))]
    internal static class PlayerDropItemPatch
    {
        private static bool Prefix(Inventory __instance, int itemInventoryIndex)
        {
            if (!__instance._iawRateLimit.CanExecute() || itemInventoryIndex < 0 ||
                    itemInventoryIndex >= __instance.items.Count) return false;

            var syncItemInfo = __instance.items[itemInventoryIndex];

            if (__instance.items[itemInventoryIndex].id != syncItemInfo.id) return false;

            var item = syncItemInfo.GetItem();

            Server.Get.Events.Player.InvokePlayerDropItemPatch(__instance.GetPlayer(), item, out var allow);

            if (!allow) return false;

            item.Drop();

            return false;
        }
    }
}
