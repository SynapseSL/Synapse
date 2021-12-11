using System;
using HarmonyLib;
using PlayerStatsSystem;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(SyncedStatBase), nameof(SyncedStatBase.CurValue), MethodType.Setter)]
    internal static class PlayerHealPatch
    {
        [HarmonyPrefix]
        private static bool OnHeal(StatBase __instance, ref float value)
        {
            try
            {
                var player = __instance.Hub.GetPlayer();
                var allow = true;
                SynapseController.Server.Events.Player.InvokePlayerHealEvent(player, ref value, ref allow);
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