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
        [HarmonyPrefix]
        private static bool ServerAddItem(ref ItemBase __result, Inventory inv, global::ItemType type, ushort itemSerial, InventorySystem.Items.Pickups.ItemPickupBase pickup)
        {
            try
            {
                __result = null;
                if (inv.UserInventory.Items.Count >= 8) return false;

                if (itemSerial == 0)
                    itemSerial = InventorySystem.Items.ItemSerialGenerator.GenerateNext();

                var itembase = inv.CreateItemInstance(type, inv.isLocalPlayer);

                if (itembase == null) return false;

                inv.UserInventory.Items[itemSerial] = itembase;
                itembase.ItemSerial = itemSerial;

                if (pickup == null) new SynapseItem(itembase);
                else
                {
                    var item = pickup.GetSynapseItem();
                    item.ItemBase = itembase;
                }

                itembase.OnAdded(pickup);
                if (inv.isLocalPlayer && itembase is InventorySystem.Items.IAcquisitionConfirmationTrigger trigger)
                    trigger.ServerConfirmAcqusition();
                inv.SendItemsNextFrame = true;
                __result = itembase;
                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Items: AddItem failed!!\n{e}");
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerCreatePickup))]
    internal static class CreatePickupPatch
    {
        [HarmonyPostfix]
        private static void ServerCreatePickupPatch(ItemPickupBase __result, InventorySystem.Items.Pickups.PickupSyncInfo psi, bool spawn = true)
        {
            try
            {
                var item = SynapseItem.AllItems[psi.Serial];

                if (item == null) item = new SynapseItem(__result);
                else item.PickupBase = __result;

                if (!spawn) item.PickupBase.transform.localScale = item.Scale;
                else item.Scale = item.Scale;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Items: CreatePickup failed!!\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    internal static class RemoveItemPatch
    {
        [HarmonyPrefix]
        private static bool ServerRemoveItemPatch(ushort itemSerial, InventorySystem.Items.Pickups.ItemPickupBase ipb)
        {
            try
            {
                //When ipb is null then this Method is used to destroy the entire object if not it is used to switch to a pickup
                if (ipb == null)
                {
                    var item = SynapseItem.AllItems[itemSerial];
                    item.Destroy();
                    return false;
                }

                return true;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Items: RemoveItem failed!!\n{e}");
                return true;
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
