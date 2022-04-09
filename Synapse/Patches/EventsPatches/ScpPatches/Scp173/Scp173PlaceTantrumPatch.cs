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
                if ((double)__instance._tantrumCooldownRemaining > 0.0 || __instance._isObserved)
                    return false;
                bool allow;
                Server.Get.Events.Scp.Scp173.InvokeScp173PlaceTantrumEvent(__instance.Hub.GetPlayer(), out allow);
                return allow;
            }
            catch (Exception ex)
            {
                Logger.Get.Error(string.Format("Synapse-Event: Scp173PlaceTantrum(Scp173) failed!!\n{0}", (object)ex));
                return true;
            }
        }
    }
}
