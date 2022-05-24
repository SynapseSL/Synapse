using System;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using Mirror;
using Synapse.Api;
using UnityEngine;
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

                Vector3 targetPos = Vector3.zero;

                Physics.Raycast(player.CameraReference.transform.position, player.CameraReference.transform.forward, out var raycastthit, 1000f);
                if (raycastthit.collider is null)
                    return true;
                
                targetPos = raycastthit.point;
                raycastthit.transform.gameObject.TryGetComponent(out Player target);

                
                Server.Get.Events.Player.InvokePlayerShootEvent(player, target, targetPos, item, out var allow);
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
                return false;
            }
        }
    }
}
