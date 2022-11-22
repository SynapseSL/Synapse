using System;
using Achievements;
using HarmonyLib;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.Coin;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Utils.Networking;
using Random = UnityEngine.Random;

namespace Synapse3.SynapseModule.Patches;

[Patches]
[HarmonyPatch]
internal static class ItemEventsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.ExecuteServerside))]
    public static bool OnMicroExecute(MicroHIDItem __instance)
    {
        try
        {
            DecoratedItemPatches.OnMicro(__instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Use Micro Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ThrowableNetworkHandler), nameof(ThrowableNetworkHandler.ServerProcessRequest))]
    public static bool OnThrow(NetworkConnection conn, ThrowableNetworkHandler.ThrowableItemRequestMessage msg)
    {
        try
        {
            DecoratedItemPatches.OnGrenade(conn, msg);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Throw Grenade Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    public static bool OnShootMsg(NetworkConnection conn, ShotMessage msg)
    {
        try
        {
            DecoratedItemPatches.OnShoot(conn, msg);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Shoot Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DisarmingHandlers), nameof(DisarmingHandlers.ServerProcessDisarmMessage))]
    public static bool OnDisarm(NetworkConnection conn, DisarmMessage msg)
    {
        try
        {
            DecoratedItemPatches.OnDisarm(conn, msg);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Disarm Event failed\n" + ex);
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
            var isTails = Random.value >= 0.5f;

            var ev = new FlipCoinEvent(player.Inventory.ItemInHand, player, isTails);
            Synapse.Get<ItemEvents>().FlipCoin.Raise(ev);

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
    [HarmonyPatch(typeof(RadioItem), nameof(RadioItem.ServerProcessCmd))]
    public static bool OnRadioCommand(RadioMessages.RadioCommand command, RadioItem __instance)
    {
        try
        {
            DecoratedItemPatches.OnRadio(command, __instance);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Radio Use Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerRequestReceived))]
    public static bool WeaponRequest(NetworkConnection conn, RequestMessage msg)
    {
        try
        {
            return DecoratedItemPatches.OnWeaponRequest(conn, msg);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Reload Weapon Event failed\n" + ex);
            return true;
        }
    }
}

internal static class DecoratedItemPatches
{
    //TODO: Find out how the Client handles the Status - changing the State causes a lot of troubles
    public static void OnMicro(MicroHIDItem micro)
    {
        var state = micro.State;
        var item = micro.GetItem();
        var removeEnergy = 0f;
        
        var ev = new MicroUseEvent(item, state switch
        {
            HidState.Firing => ItemInteractState.Finalize,
            HidState.Idle => ItemInteractState.Idle,
            HidState.PoweringDown => ItemInteractState.Cancel,
            HidState.PoweringUp => ItemInteractState.Start,
            HidState.Primed => ItemInteractState.Active,
            _ => ItemInteractState.Idle
        }, item.ItemOwner, micro.EnergyToByte, state != HidState.Idle, state);

        Synapse.Get<ItemEvents>().MicroUse.Raise(ev);

        if (!ev.Allow)
        {
            micro.State = HidState.Idle;
            micro.ServerSendStatus(HidStatusMessageType.State, (byte)micro.State);
        }
        
        switch (ev.MicroState)
        {
            case HidState.Idle:
            case HidState.StopSound:
                if (micro.RemainingEnergy > 0f && micro.UserInput != HidUserInput.None && ev.AllowChangingState)
                {
                    micro.State = HidState.PoweringUp;
                    micro._stopwatch.Restart();
                }
                break;
            
            case HidState.PoweringUp:
                if(!micro._stopwatch.IsRunning) micro._stopwatch.Restart();
                
                if (((micro.UserInput == HidUserInput.None && micro._stopwatch.Elapsed.TotalSeconds >= 0.35f) ||
                    micro.RemainingEnergy <= 0f) && ev.AllowChangingState)
                {
                    micro.State = HidState.PoweringDown;
                    micro._stopwatch.Restart();
                }
                else if (micro._stopwatch.Elapsed.TotalSeconds >= 5.95f && ev.AllowChangingState)
                {
                    micro.State = micro.UserInput == HidUserInput.Fire ? HidState.Firing : HidState.Primed;
                    micro._stopwatch.Restart();
                }

                removeEnergy =
                    micro._energyConsumtionCurve.Evaluate((float)(micro._stopwatch.Elapsed.TotalSeconds / 5.95f));
                break;
            
            case HidState.PoweringDown:
                if(!micro._stopwatch.IsRunning) micro._stopwatch.Restart();
                
                if (micro._stopwatch.Elapsed.TotalSeconds >= 3.1f && ev.AllowChangingState)
                {
                    micro.State = HidState.Idle;
                    micro._stopwatch.Stop();
                    micro._stopwatch.Reset();
                }
                break;
            
            case HidState.Primed:
                if(!micro._stopwatch.IsRunning) micro._stopwatch.Restart();
                
                if (((micro.UserInput != HidUserInput.Prime && micro._stopwatch.Elapsed.TotalSeconds >= 0.35f) ||
                    micro.RemainingEnergy <= 0f) && ev.AllowChangingState)
                {
                    micro.State = micro.UserInput == HidUserInput.Fire && micro.RemainingEnergy > 0f
                        ? HidState.Firing
                        : HidState.PoweringDown;
                    
                    micro._stopwatch.Restart();
                }
                else
                {
                    removeEnergy = micro._energyConsumtionCurve.Evaluate(1f);
                }
                break;
            
            case HidState.Firing:
                if(!micro._stopwatch.IsRunning) micro._stopwatch.Restart();

                if (micro._stopwatch.Elapsed.TotalSeconds > 1.7f)
                {
                    removeEnergy = 0.13f;
                    micro.Fire();

                    if ((micro.RemainingEnergy == 0f || (micro.UserInput != HidUserInput.Fire &&
                                                        micro._stopwatch.Elapsed.TotalSeconds >= 2.05f)) && ev.AllowChangingState)
                    {
                        micro.State = micro.RemainingEnergy > 0f && micro.UserInput == HidUserInput.Prime
                            ? HidState.Primed
                            : HidState.PoweringDown;

                        micro._stopwatch.Restart();
                    }
                }
                else
                {
                    removeEnergy = micro._energyConsumtionCurve.Evaluate(1f);
                }
                break;
        }

        if (state != micro.State)
        {
            micro.ServerSendStatus(HidStatusMessageType.State, (byte)micro.State);
        }

        if (ev.Energy != micro.EnergyToByte || removeEnergy > 0)
        {
            micro.RemainingEnergy = Mathf.Clamp01(micro.RemainingEnergy - removeEnergy * Time.deltaTime);
            micro.ServerSendStatus(HidStatusMessageType.EnergySync, micro.EnergyToByte);
        }
        
        if (ev.CanScp939Hear)
        {
            micro.Owner.scp939visionController.MakeNoise(40f);
        }
    }
    
    public static void OnGrenade(NetworkConnection connection, ThrowableNetworkHandler.ThrowableItemRequestMessage msg)
    {
        var player = connection.GetSynapsePlayer();
        if(player == null) return;
        if (player.Inventory.ItemInHand.Serial != msg.Serial ||
            player.Inventory.ItemInHand.Item is not ThrowableItem throwable) return;

        switch (msg.Request)
        {
            case ThrowableNetworkHandler.RequestType.BeginThrow:
                var ev = new ThrowGrenadeEvent(player.Inventory.ItemInHand, ItemInteractState.Start, player, false);
                Synapse.Get<ItemEvents>().ThrowGrenade.Raise(ev);
                //The Client just ignores that the server didn't "allowed to start throwing the grenade
                if (!ev.Allow)
                {
                    ForceStop(throwable, player);
                    return;
                }
                throwable.ServerProcessInitiation();
                break;
            
            case ThrowableNetworkHandler.RequestType.ConfirmThrowWeak:
            case ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce:
                ev = new ThrowGrenadeEvent(player.Inventory.ItemInHand, ItemInteractState.Finalize, player,
                    msg.Request == ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce);
                Synapse.Get<ItemEvents>().ThrowGrenade.Raise(ev);
                //If we just not generate a Grenade and doesn't delete the Item an Exception happens on the client and he can't open his inventory
                if (!ev.Allow)
                {
                    ForceStop(throwable, player);
                    return;
                }
                throwable.ServerProcessThrowConfirmation(ev.ThrowFullForce, msg.CameraPosition, msg.CameraRotation,
                    msg.PlayerVelocity);
                break;
            
            case ThrowableNetworkHandler.RequestType.CancelThrow:
                ev = new ThrowGrenadeEvent(player.Inventory.ItemInHand, ItemInteractState.Cancel, player, false);
                Synapse.Get<ItemEvents>().ThrowGrenade.Raise(ev);
                //I didn't found a way to disallow the cancel of the grenade
                throwable.ServerProcessCancellation();
                break;
        }
    }
    
    private static void ForceStop(ThrowableItem throwable, SynapsePlayer player)
    {
        throwable.CancelStopwatch.Start();
        throwable.ThrowStopwatch.Reset();
        ReCreateItem(player, player.Inventory.ItemInHand);
        new ThrowableNetworkHandler.ThrowableItemAudioMessage(throwable, ThrowableNetworkHandler.RequestType.CancelThrow).SendToAuthenticated();
    }

    private static void ReCreateItem(SynapsePlayer player, SynapseItem item)
    {
        var newItem = new SynapseItem(item.Id)
        {
            Durability = item.Durability,
            ObjectData = item.ObjectData,
            Scale = item.Scale,
            Parent = item.Parent,
            OriginalScale = item.OriginalScale,
            UpgradeProcessor = item.UpgradeProcessor,
            MoveInElevator = item.MoveInElevator,
            SchematicConfiguration = item.SchematicConfiguration,
            CanBePickedUp = item.CanBePickedUp
        };
        item.Destroy();
        newItem.EquipItem(player);
    }
    
    public static void OnShoot(NetworkConnection connection, ShotMessage msg)
    {
        var player = connection.GetSynapsePlayer();
        if (player == null) return;
        if (player.Inventory.ItemInHand.Serial != msg.ShooterWeaponSerial) return;
        if (player.Inventory.ItemInHand.Item is not Firearm firearm) return;
        var target = msg.TargetNetId == 0 ? null : Synapse.Get<PlayerService>().GetPlayer(msg.TargetNetId);
        var ev = new ShootEvent(player.Inventory.ItemInHand, ItemInteractState.Finalize, player, target,
            msg.TargetPosition, msg.TargetRotation);
        Synapse.Get<ItemEvents>().Shoot.Raise(ev);
        if(!ev.Allow) return;

        if (firearm.ActionModule.ServerAuthorizeShot())
            firearm.HitregModule.ServerProcessShot(msg);
        firearm.OnWeaponShot();
    }

    public static bool OnWeaponRequest(NetworkConnection connection, RequestMessage msg)
    {
        if (msg.Request != RequestType.Reload) return true;
        
        var player = connection.GetSynapsePlayer();
        if (player == null) return false;
        if (msg.Serial != player.Inventory.ItemInHand.Serial) return false;
        if (player.Inventory.ItemInHand.Item is not Firearm firearm) return false;

        var ev = new ReloadWeaponEvent(player.Inventory.ItemInHand, ItemInteractState.Finalize, player, false);

        Synapse.Get<ItemEvents>().ReloadWeapon.Raise(ev);
        
        if ((ev.Allow && firearm.AmmoManagerModule.ServerTryReload()) || ev.PlayAnimationOverride)
            player.SendNetworkMessage(new RequestMessage(msg.Serial, RequestType.Reload));

        return false;
    }
    
    public static void OnRadio(RadioMessages.RadioCommand command, RadioItem radio)
    {
        var item = radio.GetItem();
        var player = item.ItemOwner;
        var state = (RadioMessages.RadioRangeLevel)radio._radio.NetworkcurRangeId;

        var nextState = command == RadioMessages.RadioCommand.ChangeRange
            ? (int)state + 1 >= radio.Ranges.Length ? 0 : state + 1
            : state;

        var ev = new RadioUseEvent(item, ItemInteractState.Finalize, player, command, state, nextState);
        Synapse.Get<ItemEvents>().RadioUse.Raise(ev);
        
        if(!ev.Allow) return;
        
        switch (command)
        {
            case RadioMessages.RadioCommand.Enable:
                radio._enabled = true;
                break;
            
            case RadioMessages.RadioCommand.Disable:
                radio._enabled = false;
                radio._radio.ForceDisableRadio();
                break;
            
            case RadioMessages.RadioCommand.ChangeRange:
                radio._rangeId = (byte)ev.NextRange;
                radio._radio.NetworkcurRangeId = radio._rangeId;
                break;
        }

        if (ev.CurrentRange == ev.NextRange)
        {
            radio._rangeId = (byte)ev.NextRange;
            radio._radio.NetworkcurRangeId = radio._rangeId;   
        }
        
        radio.SendStatusMessage();
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
}