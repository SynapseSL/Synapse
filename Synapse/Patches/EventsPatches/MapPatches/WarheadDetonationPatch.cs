using HarmonyLib;
using System;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    internal static class WarheadDetonationPatch
    {
        [HarmonyPrefix]
        private static void Detonate()
        {
            try
            {
                SynapseController.Server.Events.Map.InvokeWarheadDetonationEvent();
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: WarheadDetonation failed!!\n{e}");
            }
        }
    }
}