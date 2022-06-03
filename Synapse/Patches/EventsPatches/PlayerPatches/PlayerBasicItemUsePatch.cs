using System;
using HarmonyLib;
using InventorySystem.Items.Usables;
using Mirror;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;
using Utils.Networking;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(UsableItemsController), nameof(UsableItemsController.ServerReceivedStatus))]
    internal static class ReceivedMessagePatch
    {
        [HarmonyPrefix]
        private static bool OnMessage(NetworkConnection conn, StatusMessage msg)
        {
            try
            {
                var player = conn.GetPlayer();
                if (player == null) return false;
                if (player.ItemInHand.Serial != msg.ItemSerial) return false;
                if (!(player.ItemInHand.ItemBase is UsableItem usable)) return false;
                var handler = UsableItemsController.GetHandler(player.Hub);
                var allow = true;
                var item = player.ItemInHand;
                switch (msg.Status)
                {
                    case StatusMessage.StatusType.Start:
                        if (handler.CurrentUsable.ItemSerial != 0) return false;
                        if (!usable.CanStartUsing) return false;
                        var cooldown = UsableItemsController.GetCooldown(msg.ItemSerial, usable, handler);
                        if (cooldown > 0f)
                        {
                            conn.Send(new ItemCooldownMessage(msg.ItemSerial, cooldown));
                            return false;
                        }
                        
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Initiating, ref allow);
                        if (!allow) return false;
                        
                        handler.CurrentUsable = new CurrentlyUsedItem(usable, msg.ItemSerial, Time.timeSinceLevelLoad);
                        handler.CurrentUsable.Item.OnUsingStarted();
                        new StatusMessage(StatusMessage.StatusType.Start,msg.ItemSerial).SendToAuthenticated();
                        break;
                    
                    case StatusMessage.StatusType.Cancel:
                        if (handler.CurrentUsable.ItemSerial == 0) return false;
                        if (handler.CurrentUsable.StartTime + handler.CurrentUsable.Item.MaxCancellableTime <=
                            Time.timeSinceLevelLoad) return false;
                        
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Stopping, ref allow);
                        if (!allow) return false;
                        
                        handler.CurrentUsable.Item.OnUsingCancelled();
                        handler.CurrentUsable = CurrentlyUsedItem.None;
                        new StatusMessage(StatusMessage.StatusType.Cancel, msg.ItemSerial).SendToAuthenticated();
                        break;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Receive Message failed!!\n{ex}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ServerOnUsingCompleted))]
    internal static class UsableUsingCompletePatch
    {
        internal static bool ExecuteFinalizingEvent(Consumable consumable)
        {
            var item = consumable.GetSynapseItem();
            var player = item.ItemHolder;
            var allow = true;
            
            Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Finalizing, ref allow);
            
            if (!allow)
            {
                consumable.OnUsingCancelled();
                var handler = UsableItemsController.GetHandler(consumable.Owner);
                handler.CurrentUsable = CurrentlyUsedItem.None;
                NetworkServer.SendToAll(new StatusMessage(StatusMessage.StatusType.Cancel, item.Serial), 0, false);
            }
            return allow;
        }
        
        [HarmonyPrefix]
        private static bool CompletePatch(Consumable __instance)
        {
            try
            {
                return ExecuteFinalizingEvent(__instance);
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Finalizing failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp268), nameof(Scp268.ServerOnUsingCompleted))]
    internal static class Scp268CompletePatch
    {
        [HarmonyPrefix]
        private static bool CompletePatch(Consumable __instance)
        {
            try
            {
                return UsableUsingCompletePatch.ExecuteFinalizingEvent(__instance);
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Finalizing failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Consumable), nameof(Consumable.EquipUpdate))]
    internal static class EquipPatch
    {
        [HarmonyPrefix]
        private static bool OnEquip(Consumable __instance)
        {
            try
            {
                if (__instance.ActivationReady)
                {
                    if(UsableUsingCompletePatch.ExecuteFinalizingEvent(__instance))
                        __instance.ActivateEffects();
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Finalizing Equip failed!!\n{ex}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Consumable), nameof(Consumable.OnRemoved))]
    internal static class RemovedPatch
    {
        [HarmonyPrefix]
        private static bool OnRemove(Consumable __instance,InventorySystem.Items.Pickups.ItemPickupBase pickup)
        {
            try
            {
                if(__instance.ActivationReady && UsableUsingCompletePatch.ExecuteFinalizingEvent(__instance))
                    __instance.ActivateEffects();
                
                if(__instance._alreadyActivated && pickup != null)
                    pickup.DestroySelf();
                
                if(NetworkServer.active)
                    UsableItemsController.GetHandler(__instance.Owner).CurrentUsable = CurrentlyUsedItem.None;

                return false;
            }
            catch (Exception ex)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Finalizing Remove failed!!\n{ex}");
                return true;
            }
        }
    }
}