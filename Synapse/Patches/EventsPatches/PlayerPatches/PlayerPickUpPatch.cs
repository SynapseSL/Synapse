using System;
using HarmonyLib;
using Mirror;
using Searching;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SearchCoordinator), nameof(SearchCoordinator.ContinuePickupServer))]
    internal static class PlayerPickUpPatch
    {
        private static bool Prefix(SearchCoordinator __instance)
        {
            if (__instance.Completor.ValidateUpdate())
            {
                if (!(NetworkTime.time >= __instance.SessionPipe.Session.FinishTime)) return false;

                var item = __instance.Completor.TargetPickup.GetSynapseItem();
                var player = __instance.GetPlayer();

                var allow = true;
                try
                {
                    Server.Get.Events.Player.InvokePlayerPickUpEvent(player, item, out allow);
                }
                catch (Exception e)
                {
                    Logger.Get.Error($"Synapse-Event: PlayerPickUp failed!!\n{e}");
                }

                if (!allow)
                {
                    __instance.SessionPipe.Invalidate();
                    return false;
                }

                if (item.ItemType == ItemType.Ammo556 || item.ItemType == ItemType.Ammo762 || item.ItemType == ItemType.Ammo9mm)
                {
                    __instance.Completor.Complete();
                    item.Destroy();
                    return false;
                }
                item.PickUp(player);
            }

            return false;
        }
    }
}
