using System;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using Synapse.Api;
using Synapse.Api.Items;
using Utils.Networking;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(ThrowableNetworkHandler), nameof(ThrowableNetworkHandler.ServerProcessRequest))]
    internal static class PlayerThrowGrenadePatch
    {
        [HarmonyPrefix]
        private static bool OnThrow(NetworkConnection conn, ThrowableNetworkHandler.ThrowableItemRequestMessage msg)
        {
            try
            {
                var player = conn.GetPlayer();
                if (player is null || player.ItemInHand?.Serial != msg.Serial) return false;
                if (player.ItemInHand.ItemBase is not ThrowableItem throwable) return false;
                var allow = true;

                switch (msg.Request)
                {
                    case ThrowableNetworkHandler.RequestType.BeginThrow:
                        if (!throwable.AllowHolster) return false;
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, player.ItemInHand,
                            Api.Events.SynapseEventArguments.ItemInteractState.Initiating, ref allow);
                        if (!allow)
                        {
                            ForceStop(throwable, player);
                            return false;
                        }
                        throwable.ServerProcessInitiation();
                        break;

                    case ThrowableNetworkHandler.RequestType.ConfirmThrowWeak:
                    case ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce:

                        Server.Get.Events.Player.InvokeThrowGrenade(player, player.ItemInHand, out allow);
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, player.ItemInHand,
                            Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);

                        if (!allow)
                        {
                            ForceStop(throwable, player);
                            return false;
                        }

                        return true;

                    case ThrowableNetworkHandler.RequestType.CancelThrow:
                        allow = true;
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, player.ItemInHand,
                            Api.Events.SynapseEventArguments.ItemInteractState.Stopping, ref allow);

                        if (!allow)
                        {
                            ForceStop(throwable, player);
                            return false;
                        }

                        return true;
                }
                return false;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerThrowGrenade failed!!\n{e}");
                return true;
            }
        }

        private static void ForceStop(ThrowableItem throwable, Player player)
        {
            throwable.CancelStopwatch.Start();
            throwable.ThrowStopwatch.Reset();
            ReCreateItem(player, player.ItemInHand);
            new ThrowableNetworkHandler.ThrowableItemAudioMessage(throwable, ThrowableNetworkHandler.RequestType.CancelThrow).SendToAuthenticated();
        }

        private static void ReCreateItem(Player player, SynapseItem item)
        {
            SynapseItem newitem = new(item.ID)
            {
                Durabillity = item.Durabillity,
                ItemData = item.ItemData
            };
            item.Destroy();
            newitem.PickUp(player);
        }
    }
}