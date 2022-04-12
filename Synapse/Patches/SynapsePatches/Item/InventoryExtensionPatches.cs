using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Mirror;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.Item
{
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerAddItem))]
    internal static class AddItemPatch
    {
        [HarmonyPrefix]
        private static bool ServerAddItem(ref ItemBase __result, Inventory inv, ItemType type, ushort itemSerial, ItemPickupBase pickup)
        {
            try
            {
                __result = null;

                if (inv.UserInventory.Items.Count >= 8) return false;

                var itemBase = inv.CreateItemInstance(type, inv.isLocalPlayer);
                if (itemBase == null) return false;

                SynapseItem item;
                if (itemSerial == 0 || !SynapseItem.AllItems.TryGetValue(itemSerial, out item))
                {
                    itemSerial = ItemSerialGenerator.GenerateNext();
                    itemBase.ItemSerial = itemSerial;
                    item = new SynapseItem(itemBase);
                }
                else
                    item.ItemBase = itemBase;

                inv.UserInventory.Items[itemSerial] = itemBase;
                itemBase.ItemSerial = itemSerial;
                itemBase.OnAdded(pickup);

                if (inv.isLocalPlayer && itemBase is IAcquisitionConfirmationTrigger trigger)
                    trigger.ServerConfirmAcqusition();

                inv.SendItemsNextFrame = true;

                __result = itemBase;

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
        [HarmonyPrefix]
        private static bool ServerCreatePickupPatch(ref ItemPickupBase __result, Inventory inv, ItemBase item, PickupSyncInfo psi, bool spawn = true)
        {
            try
            {
                __result = null;

                if (item == null) return false;

                var pickup = UnityEngine.Object.Instantiate(item.PickupDropModel, inv.transform.position,
                    ReferenceHub.GetHub(inv.gameObject).PlayerCameraReference.rotation * 
                    item.PickupDropModel.transform.rotation);

                //The Value to the Serial can also be null but every Serial should be as key inside AllItems
                if (!SynapseItem.AllItems.TryGetValue(psi.Serial, out var sitem)) 
                {
                    Logger.Get.Warn($"Found unregistered ItemSerial in PickupSyncInfo (CreatePickupPatch): {psi.Serial}");
                    psi.Serial = ItemSerialGenerator.GenerateNext();
                }

                pickup.NetworkInfo = psi;
                pickup.Info = psi;

                if (sitem == null) sitem = new SynapseItem(pickup);
                else sitem.PickupBase = pickup;

                pickup.transform.localScale = sitem.Scale;

                if (spawn)
                    NetworkServer.Spawn(pickup.gameObject);

                sitem.CheckForSchematic();

                pickup.InfoReceived(default, psi);

                __result = pickup;
                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Items: CreatePickup failed!!\n{e}");
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    internal static class RemoveItemPatch
    {
        [HarmonyPrefix]
        private static bool ServerRemoveItemPatch(Inventory inv, ushort itemSerial, ItemPickupBase ipb)
        {
            try
            {
                if (!inv.UserInventory.Items.TryGetValue(itemSerial, out var itembase))
                    return false;

                if (!SynapseItem.AllItems.TryGetValue(itemSerial, out var item))
                {
                    Logger.Get.Warn($"Found unregistered ItemSerial (RemoveItemPatch): {itemSerial}");
                    return false;
                }

                //When ipb is null then this Method is used to destroy the entire object if not it is used to switch to a pickup
                if (ipb == null && item.State != ItemState.Thrown)
                {
                    item.Destroy();
                }
                else
                {
                    itembase.OnRemoved(ipb);
                    if (inv.CurInstance == itembase)
                        inv.CurInstance = null;

                    UnityEngine.Object.Destroy(itembase.gameObject);

                    if (itemSerial == inv.CurItem.SerialNumber)
                        inv.NetworkCurItem = ItemIdentifier.None;

                    inv.UserInventory.Items.Remove(itemSerial);
                    inv.SendItemsNextFrame = true;
                }

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Items: RemoveItem failed!!\n{e}");
                return false;
            }
        }
    }
}
