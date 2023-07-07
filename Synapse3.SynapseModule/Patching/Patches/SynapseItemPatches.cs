using System;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using MapGeneration;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Item;
using UnityEngine;
using Utils;
using static PlayerList;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS
[Automatic]
[SynapsePatch("ItemSerialGenerator", PatchType.SynapseItem)]
public static class ItemSerialGeneratorPatch
{
    private static readonly ItemService ItemService;
    static ItemSerialGeneratorPatch() => ItemService = Synapse.Get<ItemService>();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemSerialGenerator), nameof(ItemSerialGenerator.GenerateNext))]
    public static void GenerateSerial(ushort __result) => ItemService._allItems[__result] = null;
}

[Automatic]
[SynapsePatch("DestroyPickup", PatchType.SynapseItem)]
public static class DestroyPickupPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.DestroySelf))]
    public static bool DestroyPickup(ItemPickupBase __instance)
    {
        try
        {
            if (__instance.Info.Serial == 0) return true;

            var item = __instance.GetItem();
            if (item == null) return true;

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
            SynapseLogger<Synapse>.Error("Sy3 Items: Destroy Pickup failed\n" + ex);
        }

        return true;
    }
}

[Automatic]
[SynapsePatch("ServerAddItem", PatchType.SynapseItem)]
public static class ServerAddItemPatch
{
    private static readonly ItemService ItemService;
    static ServerAddItemPatch() => ItemService = Synapse.Get<ItemService>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerAddItem))]
    public static bool ServerAddItem(ref ItemBase __result, Inventory inv, ItemType type, ushort itemSerial,
        ItemPickupBase pickup)
    {
        try
        {
            var player = inv.GetSynapsePlayer();
            if (itemSerial == 0 || !ItemService._allItems.TryGetValue(itemSerial, out var item) || item == null)
            {
                item = new SynapseItem(type);
            }

            if (item.Pickup != pickup)
            {
                item.DestroyPickup();
            }

            item.Pickup = pickup;
            item.EquipItem(player, false);
            __result = item.Item;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Items: Add Item failed\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("ServerCreatePickup", PatchType.SynapseItem)]
public static class ServerCreatePickupPatch
{
    private static readonly ItemService ItemService;
    static ServerCreatePickupPatch() => ItemService = Synapse.Get<ItemService>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerCreatePickup))]
    public static bool ServerCreatePickup(ref ItemPickupBase __result, Inventory inv, ItemBase item, PickupSyncInfo psi,
        bool spawn)
    {
        try
        {
            if (item is null) return false;

            if (!ItemService._allItems.TryGetValue(psi.Serial, out var synapseItem))
            {
                NeuronLogger.For<Synapse>()
                    .Warn("Sy3 Items: Found unregistered ItemSerial in PickupSyncInfo (CreatePickupPatch)");
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
            SynapseLogger<Synapse>.Error("Sy3 Items: Create Pickup failed\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("ServerRemoveItem", PatchType.SynapseItem)]
public static class ServerRemoveItemPatch
{
    private static readonly ItemService ItemService;
    static ServerRemoveItemPatch() => ItemService = Synapse.Get<ItemService>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerRemoveItem))]
    public static bool ServerRemoveItem(Inventory inv, ushort itemSerial, ItemPickupBase ipb)
    {
        try
        {
            if (!inv.UserInventory.Items.ContainsKey(itemSerial))
                return false;

            if (!ItemService._allItems.TryGetValue(itemSerial, out var item))
            {
                SynapseLogger<Synapse>.Warn("Found unregistered ItemSerial in Server Remove Item Patch");
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
}

[Automatic]
[SynapsePatch("SpawnPickup", PatchType.SynapseItem)]
public static class SpawnPickupPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.SpawnPickup))]
    public static bool SpawnPickup(ItemPickupBase ipb)
    {
        try
        {
            if (ipb == null) return false;
            NetworkServer.Spawn(ipb.gameObject);

            var serial = ItemSerialGenerator.GenerateNext();

            var transform = ipb.transform;
            var info = new PickupSyncInfo(ipb.Info.ItemId, transform.position, transform.rotation, ipb.Info.Weight,
                serial)
            {
                Locked = ipb.Info.Locked
            };

            InitiallySpawnedItems.Singleton.AddInitial(info.Serial);
            ipb.NetworkInfo = info;
            ipb.Info = info;
            ipb.InfoReceived(default, info);
            _ = new SynapseItem(ipb);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Items: Map Spawn Pickup failed\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("ServerThrow", PatchType.SynapseItem)]
public static class ServerThrowPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThrowableItem), nameof(ThrowableItem.ServerThrow), typeof(float), typeof(float),
        typeof(Vector3), typeof(Vector3))]
    public static bool ServerThrow(ThrowableItem __instance, float forceAmount, float upwardFactor, Vector3 torque,
        Vector3 startVel)
    {
        try
        {
            var item = __instance.GetItem();

            item.Throwable.Throw(forceAmount, upwardFactor, torque, startVel);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Items: Throw Grenade failed\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("FuseGrenade", PatchType.SynapseItem)]
public static class GrenadePatchs
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TimedGrenadePickup), nameof(TimedGrenadePickup.Update))]
    public static bool UpdateGrenadePickup(TimedGrenadePickup __instance)
    {
        try
        {
            if (__instance == null) return false;
            if (!__instance._replaceNextFrame)
                return false;

            var item = __instance.GetItem();
            if (item == null || item.Serial == 0) return false;

            item.Throwable.Fuse(__instance._attacker);

            __instance._replaceNextFrame = false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Sy3 Items: Update Grenade failed\n" + ex);
        }

        return false;
    }
}
#endif