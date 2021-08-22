using System;
using HarmonyLib;
using InventorySystem.Items.Usables;
using Mirror;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(UsableItem), nameof(UsableItem.OnUsingStarted))]
    internal static class UsableStartPatch
    {
        [HarmonyPrefix]
        private static bool StartPatch(UsableItem __instance)
        {
            try
            {
                var item = __instance.GetSynapseItem();
                var player = item.ItemHolder;
                var allow = true;

                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Initiating, ref allow);

                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Start failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(UsableItem), nameof(UsableItem.ServerOnUsingCompleted))]
    internal static class UsableUsingCompletePatch
    {
        [HarmonyPrefix]
        private static bool CompletePatch(UsableItem __instance)
        {
            try
            {
                var item = __instance.GetSynapseItem();
                var player = item.ItemHolder;
                var allow = true;

                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Finalizing, ref allow);

                if (!allow)
                {
                    (__instance as Consumable).OnUsingCancelled();
                    var handler = UsableItemsController.GetHandler(__instance.Owner);
                    handler.CurrentUsable = CurrentlyUsedItem.None;
                    NetworkServer.SendToAll(new StatusMessage(StatusMessage.StatusType.Cancel, item.Serial), 0, false);
                }
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Finalizing failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(UsableItem), nameof(UsableItem.OnUsingCancelled))]
    internal static class UsableCannelPatch
    {
        [HarmonyPrefix]
        private static bool CancelPatch(UsableItem __instance)
        {
            try
            {
                var item = __instance.GetSynapseItem();
                var player = item.ItemHolder;
                var allow = true;

                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Stopping, ref allow);

                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Cancel failed!!\n{e}");
                return true;
            }
        }
    }
}