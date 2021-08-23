using System;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using Mirror;
using Synapse.Api.Items;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    internal static class PlayerShootPatch
    {
        [HarmonyPrefix]
        private static bool ServerProcessShotPatch(NetworkConnection conn, ShotMessage msg)
        {
            try
            {
                var player = conn.GetPlayer();

                if (!player.VanillaInventory.UserInventory.Items.TryGetValue(msg.ShooterWeaponSerial, out var itembase)) return false;
                var item = itembase.GetSynapseItem();

                var target = Server.Get.Players.FirstOrDefault(x => x.NetworkIdentity.netId == msg.TargetNetId);

                Server.Get.Events.Player.InvokePlayerShootEvent(player, target, msg.TargetPosition, item, out var allow);
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
