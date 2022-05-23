﻿using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Linq;
using EventHandler = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(Locker), nameof(Locker.ServerInteract))]
    internal static class UseLockerPatch
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

                if (item?.ItemCategory is ItemCategory.Keycard)
                    EventHandler.Get.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Finalizing, ref flag);

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

        private static bool CheckPerms(Synapse.Api.Player ply, KeycardPermissions RequiredPermissions)
        {
            if (RequiredPermissions is KeycardPermissions.None)
                return true;


            if (ply is not null)
            {
                if (ply.Bypass)
                    return true;

                if (Server.Get.Configs.SynapseConfiguration.RemoteKeyCard)
                {
                    foreach (var item in ply.Inventory.Items.Where(x => x.ItemCategory is ItemCategory.Keycard))
                    {
                        if (((item.ItemBase as KeycardItem).Permissions & RequiredPermissions) == RequiredPermissions)
                        {
                            var allowcard = true;
                            EventHandler.Get.Player.InvokePlayerItemUseEvent(ply, item, ItemInteractState.Finalizing, ref allowcard);
                            if (allowcard) return allowcard;
                        }
                    }
                }
                else
                {
                    if (ply.ItemInHand is null || ply.ItemInHand.ItemBase is not KeycardItem keycardItem)
                        return false;

                    var allowcard = (keycardItem.Permissions & RequiredPermissions) == RequiredPermissions;
                    if (allowcard)
                        EventHandler.Get.Player.InvokePlayerItemUseEvent(ply, ply.ItemInHand, ItemInteractState.Finalizing, ref allowcard);
                    return allowcard;
                }
            }
            return false;
        }
    }
}