using UnityEngine;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OverridePosition))]
    internal static class OverridePositionPatch
    {
        [HarmonyPrefix]
        private static bool OverridePosition(PlayerMovementSync __instance, Vector3 pos, PlayerMovementSync.PlayerRotation? rot = null, bool forceGround = false)
        {
            try
            {
                if (forceGround && Physics.Raycast(pos, Vector3.down, out var raycastHit, 100f, __instance.CollidableSurfaces))
                    pos = raycastHit.point + Vector3.up * 1.23f * __instance.transform.localScale.y;
                __instance.ForcePosition(pos);

                if (rot is not null)
                    __instance.ForceRotation(rot.Value);
            }
            catch { }

            return false;
        }
    }
}