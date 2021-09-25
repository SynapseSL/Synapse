using System;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using Mirror;
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

                if (!player.VanillaInventory.UserInventory.Items.TryGetValue(msg.ShooterWeaponSerial, out var itembase))
                    return false;
                var item = itembase.GetSynapseItem();

                var target = Server.Get.Players.FirstOrDefault(x => x.NetworkIdentity.netId == msg.TargetNetId);
                if (target == null)
                    target = Server.Get.Map.Dummies.FirstOrDefault(x => x.Player.NetworkIdentity?.netId == msg.TargetNetId)?.Player;

                Server.Get.Events.Player.InvokePlayerShootEvent(player, target, msg.TargetPosition, item, out var allow);
                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);

                if (allow)
                {
                    if (!ReferenceHub.TryGetHub(conn.identity.gameObject, out ReferenceHub referenceHub))
                    {
                        return false;
                    }
                    if (msg.ShooterWeaponSerial != referenceHub.inventory.CurItem.SerialNumber)
                    {
                        return false;
                    }
                    if (referenceHub.inventory.CurInstance is Firearm firearm && firearm.ActionModule.ServerAuthorizeShot())
                    {
                        firearm.HitregModule.ServerProcessShot(msg);
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerShoot failed!!\n{e}");
                return true;
            }
        }
    }
}
