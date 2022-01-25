using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using System;
using System.Linq;
using EventHandler = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(Locker), nameof(Locker.ServerInteract))]

    class UseLockerPatch
    {
        [HarmonyPrefix]
        private static bool LockerInteractPatch(Locker __instance, ReferenceHub ply, byte colliderId)
        {
            try
            {
                if (colliderId >= __instance.Chambers.Length || !__instance.Chambers[colliderId].CanInteract)
                    return false;
                

                var player = ply.GetPlayer();
                var flag = CheckPerms(player, __instance.Chambers[colliderId].RequiredPermissions);
                var item = player.ItemInHand;
                var lockerChamber = __instance.GetLocker().Chambers[colliderId];

                if (item?.ItemCategory == ItemCategory.Keycard)
                    EventHandler.Get.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref flag);

                EventHandler.Get.Map.InvokeLockerInteractEvent(player, lockerChamber, ref flag);
                if (flag)
                    lockerChamber.Open = !lockerChamber.Open;
                else
                    __instance.RpcPlayDenied(colliderId);
                    
                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: UseLocker failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }

        private static bool CheckPerms(Synapse.Api.Player ply, KeycardPermissions RequiredPermissions)
        {
            if (RequiredPermissions == KeycardPermissions.None)
                return true;
            

            if (ply != null)
            {
                if (ply.Bypass)
                    return true;
                
                if (Server.Get.Configs.synapseConfiguration.RemoteKeyCard)
                {
                    foreach (var item in ply.Inventory.Items.Where(x => x.ItemCategory == ItemCategory.Keycard))
                    {
                        if (((item.ItemBase as KeycardItem).Permissions & RequiredPermissions) == RequiredPermissions)
                        {
                            var allowcard = true;
                            EventHandler.Get.Player.InvokePlayerItemUseEvent(ply, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allowcard);
                            if (allowcard) return allowcard;
                        }
                    }
                }
                else
                {
                    if (ply.ItemInHand == null || !(ply.ItemInHand.ItemBase is KeycardItem keycardItem))
                        return false;
                    
                    var allowcard = (keycardItem.Permissions & RequiredPermissions) == RequiredPermissions;
                    if (allowcard)
                        EventHandler.Get.Player.InvokePlayerItemUseEvent(ply, ply.ItemInHand, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allowcard);
                    return allowcard;
                }
            }
            return false;
        }
    }
}
