using System;
using HarmonyLib;
using Synapse.Api;
using Synapse.Patches.EventsPatches.PlayerPatches;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    internal static class RoundRestartPatch
    {
        private static void Prefix()
        {
            Map.Get.ClearObjects();

            try
            {
                Map.Get.HeavyController.Is079Recontained = false;
                Server.Get.Events.Round.InvokeRoundRestartEvent();
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: RoundRestartEvent failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}
