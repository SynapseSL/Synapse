using System;
using HarmonyLib;
using LightContainmentZoneDecontamination;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(DecontaminationController),nameof(DecontaminationController.FinishDecontamination))]
    internal static class LCZDecontaminationPatch
    {
        [HarmonyPrefix]
        private static bool OnDecontamination()
        {
            try
            {
                Server.Get.Events.Map.InvokeLCZDeconEvent(out var allow);
                return allow;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: LCZDecontamination failed!!\n{e}");
                return true;
            }
        }
    }
}
