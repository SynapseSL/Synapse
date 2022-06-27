using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Item;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class ItemPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InventorySystem.Items.ItemSerialGenerator),
        nameof(InventorySystem.Items.ItemSerialGenerator.GenerateNext))]
    public static void GenerateSerial(ushort __result) => Synapse.Get<ItemService>()._allItems[__result] = null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.DestroySelf))]
    public static bool DestroyPickupPatch(ItemPickupBase __instance)
    {
        try
        {
            var item = __instance.GetSynapseItem();
        
            //Whenever the Item should be transformed to a Inventory Item a ItemBase will be created before
            //so that when ItemBase null is the game wants to destroy it
            if (item.Item is null)
            {
                item.Destroy();
                return false;
            }
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Items: Destroy Pickup failed\n" + ex);
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerAddItem))]
    private static bool ServerAddItem(ref ItemBase __result, Inventory inv, ItemType type, ushort itemSerial,
        ItemPickupBase pickup)
    {
        try
        {
            var player = inv.GetPlayer();
            if (itemSerial == 0 || !Synapse.Get<ItemService>()._allItems.TryGetValue(itemSerial, out var item))
            {
                item = new SynapseItem(type);
            }

            if (item.Pickup != pickup)
            {
                item.DestroyPickup();
            }
            item.Pickup = pickup;
            item.EquipItem(player);
            __result = item.Item;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Items: Add Item failed\n" + ex);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerCreatePickup))]
    private static bool ServerCreatePickupPatch(ref ItemPickupBase __result, Inventory inv, ItemBase item,
        PickupSyncInfo psi, bool spawn)
    {
        try
        {
            if (item is null) return false;

            if (!Synapse.Get<ItemService>()._allItems.TryGetValue(psi.Serial, out var synapseItem))
            {
                NeuronLogger.For<Synapse>().Warn("Sy3 Items: Found unregistered ItemSerial in PickupSyncInfo (CreatePickupPatch)");
                psi.Serial = ItemSerialGenerator.GenerateNext();
            }

            if (synapseItem is null)
            {
                synapseItem = new SynapseItem(item.ItemTypeId);
            }

            synapseItem.Drop(inv.transform.position);
            __result = synapseItem.Pickup;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Items: Create Pickup failed\n" + ex);
        }

        return false;
    }
}