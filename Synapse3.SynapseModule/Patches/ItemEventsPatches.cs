using System;
using Achievements;
using HarmonyLib;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.Coin;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Radio;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using Utils.Networking;
using Random = UnityEngine.Random;

namespace Synapse3.SynapseModule.Patches;

[Patches]
public class ItemEventsPatches
{
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
    public static void OnShoot(NetworkConnection connection, ShotMessage msg)
    {
        var player = connection.GetSynapsePlayer();
        if (player == null) return;
        if (player.Inventory.ItemInHand.Serial != msg.ShooterWeaponSerial) return;
        if(player.Inventory.ItemInHand.Item is not Firearm firearm) return;
        var target = msg.TargetNetId == 0 ? null : Synapse.Get<PlayerService>().GetPlayer(msg.TargetNetId);
        var ev = new ShootEvent(player.Inventory.ItemInHand, ItemInteractState.Finalize, player, target)
        {
            Allow = firearm.ActionModule.ServerAuthorizeShot()
        };
        Synapse.Get<ItemEvents>().Shoot.Raise(ev);
        
        if(!ev.Allow) return;

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