using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Synapse.Api;
using Synapse.Api.Items;

namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerAddItem))]
    internal static class AddItemPatch
    {
        [HarmonyPostfix]
        private static void ServerAddItemPatch(ItemBase __result, ushort itemSerial = 0, ItemPickupBase pickup = null)
        {
            try
            {
                if (itemSerial == 0 || pickup == null) new SynapseItem(__result);
                else
                {
                    var item = pickup.GetSynapseItem();
                    item.ItemBase = __result;
                }
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Items: AddItem failed!!\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerCreatePickup))]
    internal static class CreatePickupPatch
    {
        [HarmonyPostfix]
        private static void ServerCreatePickupPatch(ItemPickupBase __result, InventorySystem.Items.Pickups.PickupSyncInfo psi, bool spawn = true)
        {
            var item = SynapseItem.AllItems[psi.Serial];

            if (item == null) item = new SynapseItem(__result);
            else item.PickupBase = __result;

            if (!spawn) item.PickupBase.transform.localScale = item.Scale;
            else item.Scale = item.Scale;
        }
    }

    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    internal static class RemoveItemPatch
    {
        [HarmonyPostfix]
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

    [HarmonyPatch(typeof(InventorySystem.Items.ItemSerialGenerator), nameof(InventorySystem.Items.ItemSerialGenerator.GenerateNext))]
    internal static class GenerateSerialPatch
    {
        [HarmonyPostfix]
        private static void GeneratePatch(ushort __result) => SynapseItem.AllItems[__result] = null;
    }
}
