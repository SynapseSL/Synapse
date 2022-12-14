using System;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Usables;
using InventorySystem.Items.Usables.Scp244;
using InventorySystem.Items.Usables.Scp330;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using UnityEngine;
using Utils.Networking;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
[HarmonyPatch]
internal static class UseConsumablePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.ServerReceivedStatus))]
    public static bool OnReceivedStatus(NetworkConnection conn, StatusMessage msg)
    {
        try
        {
            var player = conn.GetSynapsePlayer();
            if (player == null) return false;
            if (player.Inventory.ItemInHand.Serial != msg.ItemSerial) return false;
            if (player.Inventory.ItemInHand.Item is not UsableItem usableItem) return false;
            var handler = UsableItemsController.GetHandler(player);
            switch (msg.Status)
            {
                case StatusMessage.StatusType.Start:
                    if (handler.CurrentUsable.ItemSerial != 0) return false;
                    if (!usableItem.CanStartUsing) return false;

                    var ev = new ConsumeItemEvent(player.Inventory.ItemInHand, ItemInteractState.Start, player,
                        UsableItemsController.GetCooldown(msg.ItemSerial, usableItem, handler), handler);
                    
                    if (ev.RemainingCoolDown > 0f)
                    {
                        ev.Allow = false;
                    }

                    ev.Allow = ev.RemainingCoolDown <= 0f;

                    Synapse.Get<ItemEvents>().ConsumeItem.Raise(ev);
                    
                    if (!ev.Allow && ev.RemainingCoolDown > 0f)
                    {
                        conn.Send(new ItemCooldownMessage(msg.ItemSerial, ev.RemainingCoolDown));
                        return false;   
                    }

                    if (!ev.Allow) return false;

                    handler.CurrentUsable = new CurrentlyUsedItem(usableItem, msg.ItemSerial, Time.timeSinceLevelLoad);
                    handler.CurrentUsable.Item.OnUsingStarted();
                    new StatusMessage(StatusMessage.StatusType.Start, msg.ItemSerial).SendToAuthenticated();
                    break;
                
                case StatusMessage.StatusType.Cancel:
                    if (handler.CurrentUsable.ItemSerial == 0) return false;
                    var modifier = player.PlayerEffectsController.GetEffect<Scp1853>()
                        .GetModifierForItem(handler.CurrentUsable.Item.ItemTypeId);

                    var time = handler.CurrentUsable.StartTime +
                        handler.CurrentUsable.Item.MaxCancellableTime / modifier - Time.timeSinceLevelLoad;

                    ev = new ConsumeItemEvent(player.Inventory.ItemInHand, ItemInteractState.Cancel, player, time,
                        handler);
                    ev.Allow = time > 0;

                    Synapse.Get<ItemEvents>().ConsumeItem.Raise(ev);

                    if (ev.Allow)
                    {
                        handler.CurrentUsable.Item.OnUsingCancelled();
                        handler.CurrentUsable = CurrentlyUsedItem.None;
                        new StatusMessage(StatusMessage.StatusType.Cancel, msg.ItemSerial).SendToAuthenticated();
                    }
                    break;
            }

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Consume Item Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp330NetworkHandler), nameof(Scp330NetworkHandler.ServerSelectMessageReceived))]
    public static bool OnSelectCandy(NetworkConnection conn, SelectScp330Message msg)
    {
        try
        {
            if (msg.Drop) return true;
            var player = conn.GetSynapsePlayer();
            if (player == null) return false;
            if (player.Inventory.ItemInHand.Item is not Scp330Bag bag) return false;
            if (bag.ItemSerial != msg.Serial || msg.CandyID >= bag.Candies.Count) return false;

            var ev = new ConsumeItemEvent(player.Inventory.ItemInHand, ItemInteractState.Start, player, 0f,
                UsableItemsController.GetHandler(player));
            ev.CandyID = msg.CandyID;
            Synapse.Get<ItemEvents>().ConsumeItem.Raise(ev);
            if (!ev.Allow) return false;

            bag.SelectedCandyId = msg.CandyID;
            ev.Handler.CurrentUsable = new CurrentlyUsedItem(bag, msg.Serial, Time.timeSinceLevelLoad);
            ev.Handler.CurrentUsable.Item.OnUsingStarted();
            new StatusMessage(StatusMessage.StatusType.Start,msg.Serial).SendToAuthenticated();
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Consume Item Event failed\n" + ex);
            return true;
        }
    }

    private static bool FinalizeEvent(Consumable consumable)
    {
        var item = consumable.GetItem();
        var player = item.ItemOwner;

        var ev = new ConsumeItemEvent(item, ItemInteractState.Finalize, player, 0f,
            UsableItemsController.GetHandler(player));
        
        Synapse.Get<ItemEvents>().ConsumeItem.Raise(ev);

        if (consumable == null || item.Item == null) return false;
        if (ev.Allow) return true;

        var handler = UsableItemsController.GetHandler(player);
        handler.CurrentUsable.Item.OnUsingCancelled();
        handler.CurrentUsable = CurrentlyUsedItem.None;
        new StatusMessage(StatusMessage.StatusType.Cancel, consumable.ItemSerial).SendToAuthenticated();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ServerOnUsingCompleted))]
    public static bool CompletePatch(Consumable __instance)
    {
        try
        {
            return FinalizeEvent(__instance);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Consume Item(Complete) Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp244Item),nameof(Scp244Item.ServerOnUsingCompleted))]
    public static bool CompletePatch244(Consumable __instance)
    {
        try
        {
            return FinalizeEvent(__instance);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Consume Item(Complete) Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp268), nameof(Scp268.ServerOnUsingCompleted))]
    public static bool CompletePatch268(Consumable __instance)
    {
        try
        {
            return FinalizeEvent(__instance);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Consume Item(Complete) Event failed\n" + ex);
            return true;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.ServerOnUsingCompleted))]
    public static bool CompletePatch330(Consumable __instance)
    {
        try
        {
            return FinalizeEvent(__instance);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Consume Item(Complete) Event failed\n" + ex);
            return true;
        }
    }
}