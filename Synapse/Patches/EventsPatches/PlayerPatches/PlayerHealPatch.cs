using HarmonyLib;
using PlayerStatsSystem;
using System;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.ServerHeal))]
    internal static class PlayerHealPatch
    {
        [HarmonyPrefix]
        private static bool OnHeal(HealthStat __instance, ref float healAmount)
        {
            try
            {
                var player = __instance.Hub.GetPlayer();
                var allow = true;
                SynapseController.Server.Events.Player.InvokePlayerHealEvent(player, ref healAmount, ref allow);
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