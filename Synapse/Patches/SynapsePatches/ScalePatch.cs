using UnityEngine;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(PlayerMovementSync),nameof(PlayerMovementSync.AnticheatRaycast))]
    internal static class ScalePatchRayCast
    {
        private static void Prefix(PlayerMovementSync __instance, ref Vector3 offset) => offset.y *= __instance.transform.localScale.y;
    }

    [HarmonyPatch(typeof(PlayerMovementSync),nameof(PlayerMovementSync.AnticheatIsIntersecting))]
    internal static class ScalePatchIntersect
    {
        private static void Postfix(PlayerMovementSync __instance, ref bool __result)
        {
            if(__instance.transform.localScale.y != 1) __result = false;
        }
    }
}
