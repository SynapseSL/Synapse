using System;
using HarmonyLib;
using Synapse.Api;

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

            var item = syncItemInfo.GetSynapseItem();

            bool allow = true;
            try
            {
                Server.Get.Events.Player.InvokePlayerDropItemPatch(__instance.GetPlayer(), item, out allow);
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: DropItem failed!!\n{e}");
            }

            if (!allow) return false;

            if(item != null)
            {
                item.Drop();
            }
            else
            {
                //This Code is a Backup for the Case a Plugin creates a item on its own
                __instance.SetPickup(syncItemInfo.id, syncItemInfo.durability,
                    __instance.transform.position, __instance.camera.transform.rotation, syncItemInfo.modSight,
                    syncItemInfo.modBarrel, syncItemInfo.modOther);

                __instance.items.RemoveAt(itemInventoryIndex);
            }

            return false;
        }
    }
}
