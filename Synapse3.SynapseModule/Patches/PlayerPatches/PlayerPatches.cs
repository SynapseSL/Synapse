﻿using System;
using Achievements;
using GameCore;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.Coin;
using InventorySystem.Items.Firearms.BasicMessages;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using UnityEngine;
using Utils.Networking;

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
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Damage Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DisarmingHandlers), nameof(DisarmingHandlers.ServerProcessDisarmMessage))]
    public static bool OnDisarm(NetworkConnection conn, DisarmMessage msg)
    {
        try
        {
            DecoratedPlayerPatches.OnDisarm(conn, msg);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Disarm Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropAmmo))]
    public static bool DropAmmo(Inventory __instance, ref byte ammoType, ref ushort amount)
    {
        try
        {
            var ev = new DropAmmoEvent(__instance.GetSynapsePlayer(), true, (AmmoType)ammoType, amount);
            Synapse.Get<PlayerEvents>().DropAmmo.Raise(ev);
            ammoType = (byte)ev.AmmoType;
            amount = ev.Amount;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Drop Ammo Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem))]
    public static bool DropItem(Inventory __instance,ref ushort itemSerial, ref bool tryThrow)
    {
        try
        {
            if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var itembase) || !itembase.CanHolster())
                return false;

            var ev = new DropItemEvent(__instance.GetSynapsePlayer(), true, itembase.GetItem(), tryThrow);
            Synapse.Get<PlayerEvents>().DropItem.Raise(ev);
            tryThrow = ev.Throw;
            itemSerial = ev.ItemToDrop.Serial;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Drop Item Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    public static bool EnterFemur(CharacterClassManager __instance)
    {
        try
        {
            DecoratedPlayerPatches.EnterFemur(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Enter Femur Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CoinNetworkHandler), nameof(CoinNetworkHandler.ServerProcessMessage))]
    public static bool CoinFlip(NetworkConnection conn)
    {
        try
        {
            var player = conn.GetSynapsePlayer();
            if (player == null || !player.Hub._coinFlipRatelimit.CanExecute() ||
                player.Inventory.ItemInHand.ItemType != ItemType.Coin) return false;
            var isTails = UnityEngine.Random.value >= 0.5f;

            var ev = new FlipCoinEvent(player.Inventory.ItemInHand, player, isTails);

            if (ev.Allow)
                new CoinNetworkHandler.CoinFlipMessage(player.Inventory.ItemInHand.Serial, ev.Tails)
                    .SendToAuthenticated();
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Flip Coin Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.ServerInteract))]
    public static bool OnGeneratorInteract(Scp079Generator __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            DecoratedPlayerPatches.OnGenInteract(__instance, ply, colliderId);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: GeneratorInteract Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.ServerHeal))]
    public static bool OnHeal(HealthStat __instance, ref float healAmount)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null) return false;
            
            var ev = new HealEvent(__instance.GetSynapsePlayer(), true, healAmount);
            Synapse.Get<PlayerEvents>().Heal.Raise(ev);
            healAmount = ev.Amount;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Heal Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static void OnJoin(NicknameSync __instance, ref string nick)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player.PlayerType == PlayerType.Server) return;
            var ev = new JoinEvent(__instance.GetSynapsePlayer(), nick);
            Synapse.Get<PlayerEvents>().Join.Raise(ev);
            nick = ev.NickName;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Join Event failed\n" + ex);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    public static void OnLeave(NetworkConnection conn)
    {
        try
        {
            var player = conn.GetSynapsePlayer();
            if (player.PlayerType != PlayerType.Player) return;

            player.RemoveCustomRole(DespawnReason.Leave);

            var ev = new LeaveEvent(player);
            Synapse.Get<PlayerEvents>().Leave.Raise(ev);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Leave Event failed\n" + ex);
        }
    }
}

internal static class DecoratedPlayerPatches
{
    public static void OnGenInteract(Scp079Generator gen, ReferenceHub hub, byte interaction)
    {
        if(gen._cooldownStopwatch.IsRunning && gen._cooldownStopwatch.Elapsed.TotalSeconds < gen._targetCooldown) return;

        if (interaction != 0 && !gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open))
        {
            return;
        }
        gen._cooldownStopwatch.Stop();
        
        var player = hub.GetSynapsePlayer();

        switch (interaction)
        {
            //0 - Request interaction with Generator Doors (doors)
            case 0:
                if (gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Unlocked))
                {
                    var ev = new GeneratorInteractEvent(player, true, gen.GetSynapseGenerator(),
                        gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open)
                            ? GeneratorInteract.CloseDoor
                            : GeneratorInteract.OpenDoor);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);

                    if (ev.Allow)
                    {
                        gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Open,
                            !gen.HasFlag(gen._flags, Scp079Generator.GeneratorFlags.Open));
                        gen._targetCooldown = gen._doorToggleCooldownTime;
                    }
                }
                else
                {
                    var allow = gen._requiredPermission.CheckPermission(player);
                    var ev = new GeneratorInteractEvent(player, allow, gen.GetSynapseGenerator(),
                        GeneratorInteract.UnlockDoor);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);

                    if (ev.Allow)
                    {
                        gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, true);
                    }
                    else
                    {
                        gen.RpcDenied();
                    }

                    gen._targetCooldown = gen._unlockCooldownTime;
                }
                break;
            
            //1 - Request to swap the Activation State (lever)
            case 1:
                if ((Synapse3Extensions.CanHarmScp(player,true) || gen.Activating) && !gen.Engaged)
                {
                    var ev = new GeneratorInteractEvent(player, true, gen.GetSynapseGenerator(),
                        gen.Activating ? GeneratorInteract.Cancel : GeneratorInteract.Activate);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);
                    if(!ev.Allow) break;
                    
                    gen.Activating = !gen.Activating;
                    if (gen.Activating)
                    {
                        gen._leverStopwatch.Restart();
                    }

                    gen._targetCooldown = gen._doorToggleCooldownTime;
                }
                break;
            
            //2 - Request do Cancel The Activation (cancel button)
            case 2:
                if (gen.Activating && !gen.Engaged)
                {
                    var ev = new GeneratorInteractEvent(player, true, gen.GetSynapseGenerator(),
                        GeneratorInteract.Cancel);
                    Synapse.Get<PlayerEvents>().GeneratorInteract.Raise(ev);
                    if (!ev.Allow) break;

                    gen.ServerSetFlag(Scp079Generator.GeneratorFlags.Activating, false);
                    gen._targetCooldown = gen._unlockCooldownTime;
                }
                break;
            
            default:
                gen._targetCooldown = 1f;
                break;
        }
        
        gen._cooldownStopwatch.Restart();
    }
    
    public static void EnterFemur(CharacterClassManager manager)
    {
        if(!NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems) return;

        foreach (var player in Synapse.Get<PlayerService>().Players)
        {
            if(!player.Hub.Ready || Vector3.Distance(player.transform.position,manager._lureSpj.transform.position) >= 1.97f) continue;
            if (player.Team is Team.SCP or Team.RIP || player.GodMode) return;
            if (!Synapse3Extensions.CanHarmScp(player, false)) return;

            var service = Synapse.Get<RoundService>();
            var closeFemur = service.FemurSacrifices + 1 >=
                        Synapse.Get<SynapseConfigService>().GamePlayConfiguration.RequiredForFemur;

            var ev = new EnterFemurEvent(player, true, closeFemur);
            Synapse.Get<PlayerEvents>().EnterFemur.Raise(ev);

            if (ev.Allow)
            {
                service.FemurSacrifices++;
                player.PlayerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.UsedAs106Bait));

                if (ev.CloseFemur)
                    manager._lureSpj.SetState(false, true);
            }
        }
    }
    
    public static void OnDisarm(NetworkConnection connection, DisarmMessage msg)
    {
        if(!DisarmingHandlers.ServerCheckCooldown(connection))
            return;

        var player = connection.GetSynapsePlayer();
        if(player == null) return;

        if (!msg.PlayerIsNull)
        {
            if((msg.PlayerToDisarm.transform.position - player.transform.position).sqrMagnitude > 20f) return;
            if (msg.PlayerToDisarm.inventory.CurInstance != null &&
                msg.PlayerToDisarm.inventory.CurInstance.TierFlags != ItemTierFlags.Common) return;
        }

        var freePlayer = !msg.PlayerIsNull && msg.PlayerToDisarm.inventory.IsDisarmed();
        var canDisarm = !msg.PlayerIsNull && player.Hub.CanDisarm(msg.PlayerToDisarm);

        if (freePlayer && !msg.Disarm && player.Team != Team.SCP)
        {
            if (!player.IsDisarmed)
            {
                var ev = new FreePlayerEvent(player, true, msg.PlayerToDisarm.GetSynapsePlayer());
                Synapse.Get<PlayerEvents>().FreePlayer.Raise(ev);
                if (ev.Allow)
                    msg.PlayerToDisarm.inventory.SetDisarmedStatus(null);
            }
        }
        else
        {
            if (freePlayer || !canDisarm || !msg.Disarm)
            {
                player.SendNetworkMessage(DisarmingHandlers.NewDisarmedList);
                return;
            }

            if (msg.PlayerToDisarm.inventory.CurInstance == null ||
                msg.PlayerToDisarm.inventory.CurInstance.CanHolster())
            {
                var ev = new DisarmEvent(player.Inventory.ItemInHand, ItemInteractState.Finalize,
                    player, msg.PlayerToDisarm.GetSynapsePlayer());
                Synapse.Get<ItemEvents>().Disarm.Raise(ev);
                
                if(!ev.Allow) return;
                
                if (msg.PlayerToDisarm.characterClassManager.CurRole.team == Team.MTF &&
                    player.RoleType == RoleType.ClassD)
                {
                    AchievementHandlerBase.ServerAchieve(player.Connection,AchievementName.TablesHaveTurned);
                }

                msg.PlayerToDisarm.inventory.SetDisarmedStatus(player.VanillaInventory);
            }
        }

        DisarmingHandlers.NewDisarmedList.SendToAuthenticated();
    }
    
    public static bool OnDealDamage(PlayerStats stats, DamageHandlerBase handler)
    {
        if (!stats._hub.characterClassManager.IsAlive || stats._hub.characterClassManager.GodMode) return false;
        var damage = (handler as StandardDamageHandler)?.Damage ?? 0;

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

        var ev = new DamageEvent(player, true, attacker, damageType, damage);
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