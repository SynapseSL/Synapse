using System;
using GameCore;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.BasicMessages;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
internal static class PlayerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    public static bool OnShootMsg(NetworkConnection conn, ShotMessage msg)
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), typeof(GameObject), typeof(long), typeof(string),
        typeof(string), typeof(bool))]
    public static bool OnBan(GameObject user, long duration, string reason, string issuer, bool isGlobalBan)
    {
        try
        {
            var player = user.GetSynapsePlayer();
            var banIssuer = issuer.Contains("(")
                ? Synapse.Get<PlayerService>().GetPlayer(issuer.Substring(issuer.LastIndexOf('(') + 1,
                    issuer.Length - 2 - issuer.LastIndexOf('(')))
                : Synapse.Get<PlayerService>().GetPlayer(issuer);

            var ev = new BanEvent(player, true, banIssuer, reason, duration, isGlobalBan);
            Synapse.Get<PlayerEvents>().Ban.Raise(ev);

            return isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip") || ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Ban Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ServerSelectItem))]
    public static bool OnSelectItem(Inventory __instance, ushort itemSerial)

    {
        try
        {
            DecoratedPlayerPatches.OnChangeItem(__instance, itemSerial);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Change Item Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.DealDamage))]
    public static bool OnDamage(PlayerStats __instance, DamageHandlerBase handler)
    {
        try
        {
            return DecoratedPlayerPatches.OnDealDamage(__instance, handler);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Damage Event failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedPlayerPatches
{
    public static bool OnDealDamage(PlayerStats stats, DamageHandlerBase handler)
    {
        if (!stats._hub.characterClassManager.IsAlive || stats._hub.characterClassManager.GodMode) return false;
        var player = stats.GetSynapsePlayer();
        SynapsePlayer attacker = null;
        if (handler is AttackerDamageHandler aHandler)
            attacker = aHandler.Attacker;
        var damageType = handler.GetDamageType();

        if (damageType == DamageType.PocketDecay)
        {
            //TODO: PocketPlayers
            if (attacker != null && !Synapse3Extensions.GetHarmPermission(attacker, player)) return false;
        }

        var ev = new DamageEvent(player, true, attacker, damageType, ((StandardDamageHandler)handler).Damage);
        Synapse.Get<PlayerEvents>().Damage.Raise(ev);
        ((StandardDamageHandler)handler).Damage = ev.Damage;
        return ev.Allow;
    }
    
    public static void OnChangeItem(Inventory inventory, ushort serial)
    {
        if(serial == inventory.CurItem.SerialNumber) return;
        ItemBase oldItem = null;
        ItemBase newItem = null;
        var flag = inventory.CurItem.SerialNumber == 0 ||
                   (inventory.UserInventory.Items.TryGetValue(inventory.CurItem.SerialNumber, out oldItem) &&
                    inventory.CurInstance != null);

        if (serial == 0 || inventory.UserInventory.Items.TryGetValue(serial, out newItem))
        {
            if (inventory.CurItem.SerialNumber > 0 && flag && !oldItem.CanHolster()) return;
            if (serial != 0 && !newItem.CanEquip()) return;
            if (serial == 0)
            {
                var ev = new ChangeItemEvent(inventory.GetSynapsePlayer(), true, SynapseItem.None);
                Synapse.Get<PlayerEvents>().ChangeItem.Raise(ev);
                if (ev.Allow)
                {
                    inventory.NetworkCurItem = ItemIdentifier.None;
                    if (!inventory.isLocalPlayer)
                        inventory.CurInstance = null;
                }
            }
            else
            {
                var ev = new ChangeItemEvent(inventory.GetSynapsePlayer(), true, newItem.GetItem());
                Synapse.Get<PlayerEvents>().ChangeItem.Raise(ev);
                if (ev.Allow)
                {
                    inventory.NetworkCurItem = new ItemIdentifier(newItem.ItemTypeId, serial);
                    if (!inventory.isLocalPlayer)
                        inventory.CurInstance = newItem;
                }
            }
        }
        else if (!flag)
        {
            var ev = new ChangeItemEvent(inventory.GetSynapsePlayer(), true, SynapseItem.None);
            Synapse.Get<PlayerEvents>().ChangeItem.Raise(ev);
            if (ev.Allow)
            {
                inventory.NetworkCurItem = ItemIdentifier.None;
                if (!inventory.isLocalPlayer)
                    inventory.CurInstance = null;
            }
        }
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