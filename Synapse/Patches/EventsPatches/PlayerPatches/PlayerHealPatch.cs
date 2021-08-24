﻿using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HealHPAmount))]
    internal static class PlayerHealPatch
    {
        [HarmonyPrefix]
        private static bool OnHeal(PlayerStats __instance, ref float hp)
        {
            try
            {
                var player = __instance.GetPlayer();
                var allow = true;
                SynapseController.Server.Events.Player.InvokePlayerHealEvent(player, ref hp, ref allow);
                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerHeal failed!!\n{e}");
            }

            return true;
        }
    }
}