using System;
using HarmonyLib;
using InventorySystem.Searching;
using Mirror;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SearchCoordinator), nameof(SearchCoordinator.ContinuePickupServer))]
    internal static class PlayerPickUpPatch
    {
        private static bool Prefix(SearchCoordinator __instance)
        {
            try
            {
                var item = __instance.Completor.TargetPickup?.GetSynapseItem();

                if (item == null) return true;

                if (__instance.Completor.ValidateUpdate())
                {
                    if (NetworkTime.time < __instance.SessionPipe.Session.FinishTime) return false;

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

                    __instance.Completor.Complete();
                }
                else __instance.SessionPipe.Invalidate();

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerPickUp Patch failed!!\n{e}");
                return true;
            }
        }
    }
}
