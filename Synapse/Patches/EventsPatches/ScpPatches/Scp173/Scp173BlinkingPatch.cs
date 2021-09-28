using HarmonyLib;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp173
{
    [HarmonyPatch(typeof(PlayableScps.Scp173), nameof(PlayableScps.Scp173.ServerHandleBlinkMessage))]
    internal static class Scp173BlinkingPatch
    {
        [HarmonyPrefix]
        private static bool Blink(PlayableScps.Scp173 __instance, ref Vector3 blinkPos)
        {
            try
            {
                if (!__instance.BlinkReady) return false;
                if ((__instance.Hub.PlayerCameraReference.transform.position - blinkPos).magnitude > __instance.EffectiveBlinkDistance() * 1.05)
                    return false;

                Server.Get.Events.Scp.Scp173.InvokeScp173BlinkEvent(__instance.GetPlayer(), ref blinkPos, out var allow);
                return allow;
            }
            catch (System.Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp173BlinkEvent(Scp173) failed!!\n{e}");
                return true;
            }
        }
    }
}
