using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Neuron.Core.Meta;
using PlayerStatsSystem;
using PluginAPI.Enums;
using PluginAPI.Events;
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

[Automatic]
[SynapsePatch("GeneratorEngage", PatchType.MapEvent)]
public static class GeneratorEngagePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerUpdate))]
    public static bool GeneratorEngage(Scp079Generator __instance)
    {
        DecoratedMapPatches.GeneratorUpdate(__instance);
        return false;
    }
}

public static class DecoratedMapPatches
{
    private static readonly MapEvents MapEvents;
    static DecoratedMapPatches() => MapEvents = Synapse.Get<MapEvents>();

    public static void GeneratorUpdate(Scp079Generator generator)
    {
        var engageReady = generator._currentTime >= generator._totalActivationTime;
        if (!engageReady)
        {
            var time = Mathf.FloorToInt(generator._totalActivationTime - generator._currentTime);
            if (time != generator._syncTime)
                generator.Network_syncTime = (short)time;
        }

        if (generator.ActivationReady)
        {
            if (engageReady && !generator.Engaged)
            {
                var ev = new GeneratorEngageEvent(generator.GetSynapseGenerator());
                MapEvents.GeneratorEngage.RaiseSafely(ev);

                if (!ev.Allow || ev.ForcedUnAllow ||
                    !EventManager.ExecuteEvent(ServerEventType.GeneratorActivated, generator))
                    return;
                
                generator.Engaged = true;
                generator.Activating = false;
                return;
            }

            generator._currentTime += Time.deltaTime;
        }
        else
        {
            if(generator._currentTime == 0f || engageReady)
                return;
            
            generator._currentTime -= generator.DropdownSpeed * Time.deltaTime;
        }

        generator._currentTime = Mathf.Clamp(generator._currentTime, 0f, generator._totalActivationTime);
    }
    
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