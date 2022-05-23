using HarmonyLib;
using InventorySystem.Searching;
using Mirror;
using Synapse.Api;
using Synapse.Api.CustomObjects;
using Synapse.Api.Events.SynapseEventArguments;
using System;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SearchCoordinator), nameof(SearchCoordinator.ContinuePickupServer))]
    internal static class PlayerPickUpPatch
    {
        [HarmonyPrefix]
        private static bool OnPickup(SearchCoordinator __instance)
        {
            try
            {
                var item = __instance.Completor.TargetPickup?.GetSynapseItem();
                var schematic = item.PickupBase?.GetComponent<SynapseObjectScript>();
                var def = schematic?.Object as DefaultSynapseObject;

                if (item is null) return true;

                if (!item.CanBePickedUp && (def is null || def.Parent.ItemParent is null))
                {
                    __instance.SessionPipe.Invalidate();
                    return false;
                }

                if (__instance.Completor.ValidateUpdate())
                {
                    if (NetworkTime.time < __instance.SessionPipe.Session.FinishTime) return false;

                    var player = __instance.GetPlayer();
                    var allow = true;

                    var change = def?.Parent?.ItemParent is not null;
                    if (change)
                        item = def.Parent.ItemParent;

                    try
                    {
                        if (schematic is not null)
                        {
                            var ev = new SOPickupEventArgs
                            {
                                Object = schematic.Object as SynapseItemObject,
                                Player = player
                            };
                            Server.Get.Events.SynapseObject.InvokePickup(ev);
                            if (!ev.Allow)
                            {
                                __instance.SessionPipe.Invalidate();
                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Get.Error($"Synapse-Event: SynapseObject Pickup failed!!\n{ex}");
                    }

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

                    if (change)
                    {
                        __instance.SessionPipe.Invalidate();
                        player.Inventory.AddItem(def.Parent.ItemParent);
                        return false;
                    }

                    __instance.Completor.Complete();
                }
                else __instance.SessionPipe.Invalidate();

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerPickUp Patch failed!!\n{e}");
                return true;
            }
        }
    }
}