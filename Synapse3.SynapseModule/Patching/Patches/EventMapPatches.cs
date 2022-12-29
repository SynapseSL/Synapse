using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;
using Neuron.Core.Meta;
using Scp914;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("Scp914Upgrade", PatchType.MapEvent)]
public static class Scp914UpgradePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp914Upgrader), nameof(Scp914Upgrader.Upgrade))]
    public static bool Scp914Upgrade(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
        => DecoratedMapPatches.OnUpgrade(intake, moveVector, mode, setting);
}

public static class DecoratedMapPatches
{
    private static readonly MapEvents MapEvents;
    static DecoratedMapPatches() => MapEvents = Synapse.Get<MapEvents>();
    
    public static bool OnUpgrade(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
    {
        var list = new HashSet<GameObject>();
        var players = new List<SynapsePlayer>();
        var items = new List<SynapseItem>();
        
        var inventory = (mode & Scp914Mode.Inventory) == Scp914Mode.Inventory;
        var heldOnly = inventory && (mode & Scp914Mode.Held) == Scp914Mode.Held;

        foreach (var collider in intake)
        {
            var gameObject = collider.transform.root.gameObject;
            if (!list.Add(gameObject)) continue;

            if (gameObject.TryGetComponent<SynapsePlayer>(out var player))
            {
                players.Add(player);
            }
            else if (gameObject.TryGetComponent<ItemPickupBase>(out var pickup))
            {
                var item = pickup.GetItem();
                if (item is { CanBePickedUp: true })
                    items.Add(item);
            }
        }

        var ev = new Scp914UpgradeEvent(players, items)
        {
            MoveVector = moveVector
        };
        MapEvents.Scp914Upgrade.RaiseSafely(ev);

        if (!ev.Allow)
            return false;

        foreach (var player in ev.Players)
        {
            if (ev.MovePlayers)
                player.Position = player.transform.position + ev.MoveVector;

            if (heldOnly)
            {
                if (player.Inventory.ItemInHand != SynapseItem.None)
                {
                    player.Inventory.ItemInHand.UpgradeProcessor.CreateUpgradedItem(player.Inventory.ItemInHand,
                        setting);
                }
            }
            else if (inventory)
            {
                foreach (var item in player.Inventory.Items)
                {
                    Scp914Upgrader.OnPickupUpgraded?.Invoke(item.Pickup, setting);
                    item.UpgradeProcessor.CreateUpgradedItem(item, setting);
                }
            }

            BodyArmorUtils.RemoveEverythingExceedingLimits(player.VanillaInventory,
                player.VanillaInventory.TryGetBodyArmor(out var armor) ? armor : null);
        }

        foreach (var item in ev.Items)
        {
            item?.UpgradeProcessor.CreateUpgradedItem(item, setting,
                ev.MoveItems ? item.Position + ev.MoveVector : item.Position);
        }

        return false;
    }
}