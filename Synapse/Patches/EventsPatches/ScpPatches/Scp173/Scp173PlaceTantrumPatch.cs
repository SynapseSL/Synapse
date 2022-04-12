using HarmonyLib;
using Synapse.Api;
using System;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp173
{
    [HarmonyPatch(typeof(PlayableScps.Scp173), nameof(PlayableScps.Scp173.ServerDoTantrum))]
    internal static class Scp173PlaceTantrumPatch
    {
        [HarmonyPrefix]
        private static bool PlaceTantrum(PlayableScps.Scp173 __instance)
        {
            try
            {
                if (__instance._tantrumCooldownRemaining > 0.0f || __instance._isObserved)
                    return false;

                Server.Get.Events.Scp.Scp173.InvokeScp173PlaceTantrumEvent(__instance.Hub.GetPlayer(), out var allow);
                return allow;
            }
            catch (Exception ex)
            {
                Logger.Get.Error("Synapse-Event: Scp173PlaceTantrum(Scp173) failed!!\n" + ex);
                return true;
            }
        }
    }
}
