using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Item;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class ItemPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemSerialGenerator),
        nameof(ItemSerialGenerator.GenerateNext))]
    public static void GenerateSerial(ushort __result) => Synapse.Get<ItemService>()._allItems[__result] = null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.DestroySelf))]
    public static bool DestroyPickup(ItemPickupBase __instance)
    {
        try
        {
            var item = __instance.GetItem();
        
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
    public static bool ServerAddItem(ref ItemBase __result, Inventory inv, ItemType type, ushort itemSerial,
        ItemPickupBase pickup)
    {
        try
        {
            var player = inv.GetSynapsePlayer();
            if (itemSerial == 0 || !Synapse.Get<ItemService>()._allItems.TryGetValue(itemSerial, out var item) || item == null)
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
    public static bool ServerCreatePickup(ref ItemPickupBase __result, Inventory inv, ItemBase item,
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

            if (synapseItem == null)
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    public static bool ServerRemoveItem(Inventory inv, ushort itemSerial, ItemPickupBase ipb)
    {
        try
        {
            if (!inv.UserInventory.Items.ContainsKey(itemSerial))
                return false;

            if (!Synapse.Get<ItemService>()._allItems.TryGetValue(itemSerial, out var item))
            {
                NeuronLogger.For<Synapse>().Warn("Found unregistered ItemSerial in Server Remove Item Patch");
                return false;
            }

            //When ipb is null then this Method is used to destroy the entire object if not it is used to switch to a pickup
            if (ipb == null)
            {
                item.Destroy();
                return false;
            }
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Items: Remove Item failed\n" + ex);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.SpawnPickup))]
    public static bool SpawnPickup(ItemPickupBase ipb)
    {
        try
        {
            if (ipb == null) return false;
            NetworkServer.Spawn(ipb.gameObject);

            var serial = ItemSerialGenerator.GenerateNext();

            var info = new PickupSyncInfo
            {
                ItemId = ipb.Info.ItemId,
                Position = ipb.transform.position,
                Rotation = new LowPrecisionQuaternion(ipb.transform.rotation),
                Serial = serial,
                Weight = ipb.Info.Weight,
                Locked = ipb.Info.Locked
            };

            ipb.NetworkInfo = info;
            ipb.Info = info;
            ipb.InfoReceived(default, info);
            _ = new SynapseItem(ipb);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Items: Map Spawn Pickup failed\n" + ex);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThrowableItem), nameof(ThrowableItem.ServerThrow),
        new[] { typeof(float), typeof(float), typeof(Vector3), typeof(Vector3) })]
    private static bool ServerThrow(ThrowableItem __instance, float forceAmount, float upwardFactor, Vector3 torque,
        Vector3 startVel)
    {
        try
        {
            var item = __instance.GetItem();

            item.Throwable.Throw(forceAmount, upwardFactor, torque, startVel);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Items: Throw Grenade failed\n" + ex);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TimedGrenadePickup), nameof(TimedGrenadePickup.Update))]
    private static bool UpdateGrenadePickup(TimedGrenadePickup __instance)
    {
        try
        {
            if (!__instance._replaceNextFrame)
                return false;

            var item = __instance.GetItem();

            item.Throwable.Fuse(__instance._attacker);
            
            __instance._replaceNextFrame = false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Items: Update Grenade failed\n" + ex);
        }

        return false;
    }
}