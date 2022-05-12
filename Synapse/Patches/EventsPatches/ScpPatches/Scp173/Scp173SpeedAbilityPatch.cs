using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp173
{
    [HarmonyPatch(typeof(PlayableScps.Scp173), nameof(PlayableScps.Scp173.ServerDoBreakneckSpeeds))]
    internal static class Scp173SpeedAbilityPatch
    {
        [HarmonyPrefix]
        private static bool ServerScp173SpeedAbilityPatch(PlayableScps.Scp173 __instance)
        {
            try
            {
                SynapseController.Server.Events.Scp.Scp173.InvokeScp173BreakNeckEvent(__instance.GetPlayer(), out bool allow);
                return allow;
            }
            catch(Exception e)
            {
                Api.Logger.Get.Error($"Synapse-Event: Scp173BreackNeckPatch(Scp173) failed!!\n{e}");
                return true;
            }
        }
    }
}
