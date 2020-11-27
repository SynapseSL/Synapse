using System;
using HarmonyLib;
using Synapse.Api;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(WeaponManager),nameof(WeaponManager.CallCmdShoot))]
    internal static class PlayerShootPatch
    {
        private static bool Prefix(WeaponManager __instance, GameObject target, Vector3 targetPos)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                    return false;
                int itemIndex = __instance._hub.inventory.GetItemIndex();
                if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count || __instance.curWeapon < 0 ||
                    ((__instance._reloadCooldown > 0.0 || __instance._fireCooldown > 0.0) &&
                     !__instance.isLocalPlayer) ||
                    (__instance._hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID ||
                     __instance._hub.inventory.items[itemIndex].durability <= 0.0))
                    return false;

                Player targetplayer = null;
                if (target != null)
                    targetplayer = target.GetPlayer();

                var player = __instance.GetPlayer();
                var item = player.ItemInHand;

                Server.Get.Events.Player.InvokePlayerShootEvent(player, targetplayer, targetPos, item, out var allow);
                if (item != null)
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
