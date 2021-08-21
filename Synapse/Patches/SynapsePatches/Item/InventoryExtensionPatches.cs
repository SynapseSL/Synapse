using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Synapse.Api;
using Synapse.Api.Items;

namespace Synapse.Patches.SynapsePatches.Item
{
    internal static class InventoryExtensionPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryExtensions),nameof(InventoryExtensions.ServerAddItem))]
        private static void ServerAddItemPatch(ItemBase __result, ushort itemSerial = 0, ItemPickupBase pickup = null)
        {
            try
            {
                if (itemSerial == 0 || pickup == null) new SynapseItem(__result);
                else
                {
                    var item = SynapseItem.AllItems[itemSerial];
                    item.ItemBase = __result;
                }
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Items: AddItem failed!!\n{e}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerCreatePickup))]
        private static void ServerCreatePickupPatch(ItemPickupBase __result, ItemBase item, bool spawn = true)
        {
            var sitem = item.GetSynapseItem();

            if (sitem == null) sitem = new SynapseItem(__result);
            else sitem.PickupBase = __result;

            if (!spawn) sitem.PickupBase.transform.localScale = sitem.Scale;
            else sitem.Scale = sitem.Scale;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryExtensions),nameof(InventoryExtensions.ServerRemoveItem))]
        private static void ServerRemoveItemPatch(ushort itemSerial, InventorySystem.Items.Pickups.ItemPickupBase ipb)
        {
            //When ipb is null then this Method is used to destroy the entire object if not it is used to switch to a pickup
            if(ipb == null)
            {
                var item = SynapseItem.AllItems[itemSerial];
                item.Destroy();
            }
        }
    }
}
