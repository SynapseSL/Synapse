using System;
using Harmony;
using Mirror;
using Searching;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SearchCoordinator), nameof(SearchCoordinator.ContinuePickupServer))]
    internal class PlayerPickUpPatch
    {
        private static bool Prefix(SearchCoordinator __instance)
        {
            try
            {
                if (__instance.Completor.ValidateUpdate())
                {
                    if (!(NetworkTime.time >= __instance.SessionPipe.Session.FinishTime)) return false;

                    var item = __instance.Completor.TargetPickup.GetItem();
                    var player = __instance.GetPlayer();

                    Server.Get.Events.Player.InvokePlayerPickUpEvent(player, item, out var allow);

                    if (!allow) return false;

                    if (item != null)
                        item.PickUp(player);
                    //TODO: Remove This Code after fully implementing Custom Items into Synapse
                    else
                        __instance.Completor.Complete();
                }
                else __instance.SessionPipe.Invalidate();
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerPickUp failed!!\n{e}");
            }

            return false;
        }
    }
}
