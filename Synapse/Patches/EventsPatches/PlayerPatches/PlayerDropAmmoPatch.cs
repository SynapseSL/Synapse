using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(AmmoBox),nameof(AmmoBox.CallCmdDrop))]
    internal static class PlayerDropAmmoPatch
    {
        private static bool Prefix(AmmoBox __instance, int type, uint toDrop)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true)) return false;

                toDrop = Math.Min(toDrop, __instance.amount[type]);
                if (toDrop < 15u) return false;

                var player = __instance.GetPlayer();
                var item = player.ItemInHand;

                SynapseController.Server.Events.Player.InvokePlayerDropAmmoPatch(player, item, ref toDrop, ref type, out var allow);
                if (item != null)
                    SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);

                __instance.amount[type] -= toDrop;
                __instance._inv.SetPickup(__instance.types[type].inventoryID, toDrop, __instance.transform.position, __instance._inv.camera.transform.rotation, 0, 0, 0);
                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: PlayerAmmoDrop failed!!\n{e}");
                return false;
            }
        }
    }
}
