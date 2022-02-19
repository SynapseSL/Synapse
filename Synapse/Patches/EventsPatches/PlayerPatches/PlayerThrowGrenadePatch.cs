using System;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(ThrowableNetworkHandler), nameof(ThrowableNetworkHandler.ServerProcessMessages))]
    internal static class PlayerThrowGrenadePatch
    {
        [HarmonyPrefix]
        private static bool OnThrow(NetworkConnection conn, ThrowableNetworkHandler.ThrowableItemMessage msg)
        {
            try
            {
                var player = conn.GetPlayer();
                if (player == null || player.ItemInHand?.Serial != msg.Serial) return false;
                if (!(player.ItemInHand.ItemBase is ThrowableItem throwable)) return false;

                switch (msg.Request)
                {
                    case ThrowableNetworkHandler.RequestType.BeginThrow:
                        if (throwable.ActivationStopwatch.IsRunning) return false;

                        Server.Get.Events.Player.InvokeThrowGrenade(player, player.ItemInHand, out var allow);
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, player.ItemInHand, Api.Events.SynapseEventArguments.ItemInteractState.Initiating, ref allow);
                        if (!allow)
                        {
                            var item = player.ItemInHand;
                            //A Newitem with a new Serial is needed or else the client will try to throw it locally
                            var newitem = new Synapse.Api.Items.SynapseItem(item.ID);
                            newitem.Durabillity = item.Durabillity;
                            newitem.ItemData = item.ItemData;
                            item.Destroy();
                            newitem.PickUp(player);
                            return false;
                        }
                        throwable.ActivationStopwatch.Start();
                        break;

                    case ThrowableNetworkHandler.RequestType.ConfirmThrowWeak:
                    case ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce:
                        if (msg.Request - ThrowableNetworkHandler.RequestType.ConfirmThrowWeak > 1) return false;
                        if (throwable.ActivationStopwatch.Elapsed.TotalSeconds < throwable.MinimalAnimationTime * 0.8f) return false;

                        allow = true;
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, player.ItemInHand, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);
                        if (!allow)
                        {
                            var item = player.ItemInHand;
                            var newitem = new Synapse.Api.Items.SynapseItem(item.ID);
                            newitem.Durabillity = item.Durabillity;
                            item.Destroy();
                            newitem.PickUp(player);
                            return false;
                        }
                        throwable.ActivationStopwatch.Start();

                        var pos = player.CameraReference.position;
                        var rot = player.CameraReference.rotation;
                        var bounds = player.PlayerMovementSync.Tracer.GenerateBounds(0.1f, false);
                        bounds.Encapsulate(pos + player.PlayerMovementSync.PlayerVelocity * 0.2f);
                        player.CameraReference.position = bounds.ClosestPoint(msg.CameraPosition);
                        player.CameraReference.rotation = msg.CameraRotation;
                        throwable.ServerThrow(msg.Request == ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce, ThrowableNetworkHandler.GetLimitedVelocity(msg.PlayerVelocity));
                        player.CameraReference.position = pos;
                        player.CameraReference.rotation = rot;
                        break;
                }
                return false;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerThrowGrenade failed!!\n{e}");
                return true;
            }
        }
    }
}
