using UnityEngine;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(PlayerMovementSync),nameof(PlayerMovementSync.OverridePosition))]
    internal static class OverridePositionPatch
    {
        private static bool Prefix(PlayerMovementSync __instance, Vector3 pos, float rot, bool forceGround = false)
        {
            try
            {
                if (forceGround && Physics.Raycast(pos, Vector3.down, out var raycastHit, 100f, __instance.CollidableSurfaces))
                {
                    pos = raycastHit.point + Vector3.up * 1.23f * __instance.transform.localScale.y;
                }
                __instance.ForcePosition(pos);
                __instance.TargetSetRotation(__instance.connectionToClient, rot);
            }
            catch { }

            return false;
        }
    }
}
