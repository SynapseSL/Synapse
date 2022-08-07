using System;
using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Neuron.Core.Logging;
using Scp914;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class MapPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp914Upgrader), nameof(Scp914Upgrader.Upgrade))]
    public static bool OnUpgrade(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
    {
        try
        {
            return DecoratedMapPatches.OnUpgrade(intake, moveVector, mode, setting);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: SCP-914 Upgrade Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerUpdate))]
    public static bool OnGeneratorUpdateForEngage(Scp079Generator __instance)
    {
        try
        {
            DecoratedMapPatches.GeneratorUpdate(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Generator Engage Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInRange))]
    public static bool OnTeslaRange(TeslaGate __instance, out bool __result, ReferenceHub player)
    {
        __result = false;
        try
        {
            var sPlayer = player.GetSynapsePlayer();

            if (__instance.InRange(sPlayer.Position))
            {
                __result = !sPlayer.Invisible;
                
                var ev = new TriggerTeslaEvent(sPlayer, __result, __instance.GetSynapseTesla());
                Synapse.Get<MapEvents>().TriggerTesla.Raise(ev);

                __result = ev.Allow;
            }

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Trigger Tesla Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    public static void OnDetonate()
    {
        try
        {
            Synapse.Get<MapEvents>().DetonateWarhead.Raise(new DetonateWarheadEvent());
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Detonate Warhead Event failed\n" + ex);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), typeof(GameObject))]
    public static bool OnCancelWarhead(AlphaWarheadController __instance, GameObject disabler)
    {
        try
        {
            return DecoratedMapPatches.OnCancelWarhead(__instance, disabler);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Cancel Warhead Event failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedMapPatches
{
    public static bool OnCancelWarhead(AlphaWarheadController controller, GameObject playerObject)
    {
        if (!controller.inProgress || controller.timeToDetonation <= 10.0 || controller._isLocked) return false;

        var ev = new CancelWarheadEvent(playerObject?.GetSynapsePlayer(), true);
        Synapse.Get<MapEvents>().CancelWarhead.Raise(ev);
        return ev.Allow;
    }
    
    public static void GeneratorUpdate(Scp079Generator generator)
    {
        var engageReady = generator._currentTime >= generator._totalActivationTime;
        if (engageReady)
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
                
                if(!ev.Allow || ev.ForcedUnAllow)
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

        var ev = new Scp914UpgradeEvent(players, items)
        {
            MoveVector = moveVector
        };
        Synapse.Get<MapEvents>().Scp914Upgrade.Raise(ev);

        if (!ev.Allow)
            return false;

        foreach (var player in players)
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

        foreach (var item in items)
        {
            item?.UpgradeProcessor.CreateUpgradedItem(item, setting,
                ev.MoveItems ? item.Position + ev.MoveVector : item.Position);
        }

        return false;
    }
}