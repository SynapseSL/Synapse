﻿using System;
using HarmonyLib;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    //TODO:
    //[HarmonyPatch(typeof(AnimationController), nameof(AnimationController.Syncda))]
    internal static class SyncDataPatch
    {
        [HarmonyPrefix]
        private static bool SyncData(AnimationController __instance)
        {
            try
            {
                Server.Get.Events.Player.InvokePlayerSyncDataEvent(__instance.GetPlayer(), out bool allow);

                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSyncData failed!!\n{e}");
                return true;
            }
        }
    }
}
