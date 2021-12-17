using System;
using HarmonyLib;
using RoundRestarting;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(RoundRestart), nameof(RoundRestart.InitiateRoundRestart))]
    internal static class RoundRestartPatch
    {
        [HarmonyPrefix]
        private static void Restart()
        {
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
