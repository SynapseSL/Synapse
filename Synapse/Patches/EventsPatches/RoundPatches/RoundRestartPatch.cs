using System;
using Harmony;
using Synapse.Api;
using Synapse.Patches.EventsPatches.PlayerPatches;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    internal static class RoundRestartPatch
    {
        private static void Prefix()
        {
            var map = Map.Get;
            map.Teslas.Clear();
            map.Doors.Clear();
            map.Elevators.Clear();
            map.Rooms.Clear();
            map.Generators.Clear();
            PlayerBasicItemUsePatch.HealCache.Clear();

            try
            {
                Server.Get.Events.Round.InvokeRoundRestartEvent();
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: RoundRestartEvent failed!!\n{e}");
            }
        }
    }
}
