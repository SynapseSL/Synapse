using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using Synapse.Api;
using System;
using System.Linq;
using EventHandler = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;
using VanillaLocker = MapGeneration.Distributors.Locker;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(VanillaLocker), nameof(VanillaLocker.ServerInteract))]
    internal static class UseLockerPatch
    {
        [HarmonyPrefix]
        private static bool LockerInteractPatch(VanillaLocker __instance, ReferenceHub ply, byte colliderId)
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
                Logger.Get.Error($"Synapse-Event: UseLocker failed!!\n{e}");
                return true;
            }
        }

        private static bool CheckPerms(Player player, KeycardPermissions RequiredPermissions)
        {
            if (RequiredPermissions == KeycardPermissions.None)
                return true;

            if (player != null)
            {
                if (player.Bypass)
                    return true;

                if (Server.Get.Configs.SynapseConfiguration.RemoteKeyCard)
                {
                    foreach (var item in player.Inventory.Items.Where(x => x.ItemCategory == ItemCategory.Keycard))
                    {
                        if (((item.ItemBase as KeycardItem).Permissions & RequiredPermissions) == RequiredPermissions)
                        {
                            var allowcard = true;
                            EventHandler.Get.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allowcard);
                            if (allowcard)
                                return allowcard;
                        }
                    }
                }
                else
                {
                    if (player.ItemInHand is null || !(player.ItemInHand.ItemBase is KeycardItem keycardItem))
                        return false;

                    var allowcard = (keycardItem.Permissions & RequiredPermissions) == RequiredPermissions;
                    if (allowcard)
                        EventHandler.Get.Player.InvokePlayerItemUseEvent(player, player.ItemInHand, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allowcard);
                    return allowcard;
                }
            }

            return false;
        }
    }
}
