using System;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.BasicMessages;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
internal static class PlayerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    private static bool OnShootMsg(NetworkConnection conn, ShotMessage msg)
    {
        try
        {
            var ev = new ShootEvent(conn.GetSynapsePlayer(), msg.TargetNetId, msg.ShooterWeaponSerial, true);
            Synapse.Get<PlayerEvents>().Shoot.Raise(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Shoot Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract),nameof(PlayerInteract.UserCode_CmdDetonateWarhead))]
    public static bool OnStartWarhead(PlayerInteract __instance)
    {
        try
        {
            DecoratedPlayerPatches.OnStartWarhead(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Start Warhead Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), typeof(GameObject))]
    public static bool OnCancelWarhead(AlphaWarheadController __instance, GameObject disabler)
    {
        try
        {
            return DecoratedPlayerPatches.OnCancelWarhead(__instance, disabler);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Cancel Warhead Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
    public static bool OnDoorInteract(DoorVariant __instance,ReferenceHub ply, byte colliderId)
    {
        try
        {
            DecoratedPlayerPatches.OnDoorInteract(__instance, ply, colliderId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Door Interact Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Locker), nameof(Locker.ServerInteract))]
    public static bool OnUseLocker(Locker __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            DecoratedPlayerPatches.LockerUse(__instance, ply, colliderId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Locker Use Event failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedPlayerPatches
{
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
        Synapse.Get<PlayerEvents>().DoorInteract.Raise(ev);
        
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
    
    public static void LockerUse(Locker locker, ReferenceHub hub, byte colliderId)
    {
        if(colliderId >= locker.Chambers.Length || !locker.Chambers[colliderId].CanInteract) return;

        var player = hub.GetSynapsePlayer();
        var synapseLocker = locker.GetSynapseLocker();
        var chamber = synapseLocker.Chambers[colliderId];

        if (!locker.Chambers[colliderId].RequiredPermissions.CheckPermission(player))
        {
            locker.RpcPlayDenied(colliderId);
            return;
        }

        var ev = new LockerUseEvent(player, true, synapseLocker, chamber);
        Synapse.Get<PlayerEvents>().LockerUse.Raise(ev);

        if (!ev.Allow)
        {
            locker.RpcPlayDenied(colliderId);
            return;
        }

        chamber.Open = !chamber.Open;
    }
    
    public static bool OnCancelWarhead(AlphaWarheadController controller, GameObject playerObject)
    {
        if (!controller.inProgress || controller.timeToDetonation <= 10.0 || controller._isLocked) return false;

        var ev = new CancelWarheadEvent(playerObject.GetSynapsePlayer(), true);
        Synapse.Get<PlayerEvents>().CancelWarhead.Raise(ev);
        return ev.Allow;
    }
    
    public static void OnStartWarhead(PlayerInteract player)
    {
        if(!player.CanInteract) return;
        
        if(!player._playerInteractRateLimit.CanExecute(true)) return;
        
        var panel = GameObject.Find("OutsitePanelScript");
        var sPlayer = player.GetSynapsePlayer();

        var ev = new StartWarheadEvent(sPlayer, true);

        if (!player.ChckDis(panel.transform.position) || !AlphaWarheadOutsitePanel.nukeside.enabled ||
            !panel.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
        {
            ev.Allow = false;
        }

        Synapse.Get<PlayerEvents>().StartWarhead.Raise(ev);
        
        if(!ev.Allow) return;

        AlphaWarheadController.Host.StartDetonation();
        ServerLogs.AddLog(ServerLogs.Modules.Warhead,
            sPlayer.Hub.LoggedNameFromRefHub() + " started the Alpha Warhead detonation.",
            ServerLogs.ServerLogType.GameEvent);
        player.OnInteract();
    }
}