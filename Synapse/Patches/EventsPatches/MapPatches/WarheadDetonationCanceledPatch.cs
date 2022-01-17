using System;
using HarmonyLib;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), typeof(GameObject))]
    public class WarheadDetonationCanceledPatch
    {
        [HarmonyPrefix]
        private static bool CancelDetonation(AlphaWarheadController __instance, GameObject disabler)
        {
            try
            {
                if (!__instance.inProgress || __instance.timeToDetonation <= 10.0 || __instance._isLocked)
                    return true;
                
                SynapseController.Server.Events.Map.InvokeWarheadDetonationCanceledEvent(out bool allow, ref disabler);
                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: WarheadDetonation failed!!\n{e}");
                return true;
            }
        }
    }
}