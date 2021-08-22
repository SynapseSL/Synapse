using System;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using Synapse.Api.Items;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(IHitregModule), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    internal static class PlayerShootPatch
    {
        [HarmonyPrefix]
        private static bool ServerProcessShotPatch(ShotMessage message)
        {
            try
            {
                var item = SynapseItem.AllItems[message.ShooterWeaponSerial];
                var player = item.ItemHolder;
                var target = Server.Get.Players.FirstOrDefault(x => x.NetworkIdentity.netId == message.TargetNetId);

                Server.Get.Events.Player.InvokePlayerShootEvent(player, target, message.TargetPosition, item, out var allow);
                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);

                return allow;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerShoot failed!!\n{e}");
                return true;
            }
        }
    }
}
