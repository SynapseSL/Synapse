using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;
using Neuron.Core.Logging;
using Scp914;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Scp914;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class MapPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp914Upgrader), nameof(Scp914Upgrader.Upgrade))]
    private static bool OnUpgrade(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
    {
        try
        {
            var inventory = (mode & Scp914Mode.Inventory) == Scp914Mode.Inventory;
            var heldOnly = inventory && (mode & Scp914Mode.Held) == Scp914Mode.Held;
            var list = new List<GameObject>();
            var players = new List<SynapsePlayer>();
            var items = new List<SynapseItem>();
            
            foreach (var collider in intake)
            {
                var gameObject = collider.transform.root.gameObject;
                if(list.Contains(gameObject)) continue;
                list.Add(gameObject);

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

            var ev = new Scp914UpgradeEvent(players, items);
            Synapse.Get<MapEvents>().Scp914Upgrade.Raise(ev);

            if (!ev.Allow)
                return false;

            foreach (var player in players)
            {
                if (ev.MovePlayers)
                    player.Position = player.transform.position + moveVector;

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

            foreach (var item in items)
            {
                item?.UpgradeProcessor.CreateUpgradedItem(item, setting,
                    ev.MoveItems ? item.Position + moveVector : item.Position);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: SCP-914 Upgrade Event failed\n" + ex);
            return true;
        }
    }
}