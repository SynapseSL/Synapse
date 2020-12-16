using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    internal static class WarheadDetonationPatch
    {
        private static void Prefix()
        {
            try
            {
                SynapseController.Server.Events.Map.InvokeWarheadDetonationEvent();
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: WarheadDetonation failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}