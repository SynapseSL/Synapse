using System;
using System.Collections.Generic;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
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
    private static bool OnUpgrade(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
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
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
    public static bool OnDoorInteract(DoorVariant __instance,ReferenceHub ply, byte colliderId)
    {
        try
        {
            DecoratedMapPatches.OnDoorInteract(__instance, ply, colliderId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Door Interact Event failed\n" + ex);
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
    [HarmonyPatch(typeof(Lift), nameof(Lift.UseLift))]
    public static bool OnUseLift(Lift __instance, out bool __result)
    {
        __result = false;
        try
        {
            if (!__instance.operative || AlphaWarheadController.Host.timeToDetonation == 0f ||
                __instance._locked) return false;

            __instance.GetSynapseElevator().MoveToNext();
            __instance.operative = false;
            __result = true;
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Generator Engage Event failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedMapPatches
{
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

    public static void OnDoorInteract(DoorVariant door, ReferenceHub hub, byte colliderId)
    {
        var player = hub.GetSynapsePlayer();
        var allow = false;
        var bypassDenied = false;
        if (door.ActiveLocks > 0)
        {
            var mode = DoorLockUtils.GetMode((DoorLockReason)door.ActiveLocks);
            
            var canInteractGeneral = mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.CanOpen);
            var scpOverride = mode.HasFlagFast(DoorLockMode.ScpOverride) &&
                              hub.characterClassManager.CurRole.team == Team.SCP;
            var canChangeCurrently = mode != DoorLockMode.FullLock &&
                                     ((door.TargetState && mode.HasFlagFast(DoorLockMode.CanClose)) ||
                                      (!door.TargetState && mode.HasFlagFast(DoorLockMode.CanOpen)));

            if (!canInteractGeneral && !scpOverride && !canChangeCurrently)
            {
                bypassDenied = true;
            }
        }

        //This is most often false when the Animation is still playing
        if(!door.AllowInteracting(hub,colliderId)) return;
        
        if (!bypassDenied)
        {
            if (hub.characterClassManager.CurClass == RoleType.Scp079 ||
                door.RequiredPermissions.CheckPermission(player))
            {
                allow = true;
            }  
        }

        var ev = new DoorInteractEvent(player, allow, door.GetSynapseDoor(), bypassDenied);
        Synapse.Get<MapEvents>().DoorInteract.Raise(ev);
        
        if (ev.Allow)
        {
            door.NetworkTargetState = !door.TargetState;
            door._triggerPlayer = hub;
            return;
        }

        if (ev.LockBypassRejected)
        {
            door.LockBypassDenied(hub, colliderId);
            return;
        }
        
        door.PermissionsDenied(hub, colliderId);
        DoorEvents.TriggerAction(door, DoorAction.AccessDenied, hub); 
    }

    public static bool CheckKeyCardPerm(SynapsePlayer player)
    {
        return true;
    }
}