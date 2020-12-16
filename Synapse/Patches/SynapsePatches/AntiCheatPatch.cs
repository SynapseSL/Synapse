using System;
using HarmonyLib;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(PlayerMovementSync),nameof(PlayerMovementSync.AnticheatIsIntersecting))]
    internal static class AntiCheatPatch
    {
        private static bool Prefix(PlayerMovementSync __instance, out bool __result, Vector3 pos)
        {
            __result = false;
            try
            {
                var player = __instance.GetPlayer();
                if (player == null) return false;

                var pos1 = Vector3.zero;
                var pos2 = Vector3.zero;
                var radius = 0f;
                switch (player.RoleType)
                {
                    case RoleType.Scp93953:
                    case RoleType.Scp93989:
                        pos1 = pos - (PlayerMovementSync._yAxisOffset939 * player.Scale.y);
                        pos2 = pos + (PlayerMovementSync._yAxisOffset939 * player.Scale.y);
                        radius = 0.15f;
                        break;

                    case RoleType.ChaosInsurgency:
                        pos1 = pos - (PlayerMovementSync._yAxisOffsetCi * player.Scale.y);
                        pos2 = pos + (PlayerMovementSync._yAxisOffsetCi * player.Scale.y);
                        radius = 0.32f;
                        break;

                    default:
                        pos1 = pos - (PlayerMovementSync._yAxisOffset * player.Scale.y);
                        pos2 = pos + (PlayerMovementSync._yAxisOffset * player.Scale.y);
                        radius = 0.38f;
                        break;
                }

                var number = Physics.OverlapCapsuleNonAlloc(pos1, pos2, radius, PlayerMovementSync._sphereHits, PlayerMovementSync._r3CollidableSurfaces);

                for(int i = 0; i < number; i++)
                {
                    PlayableScps.Scp096 scp;
                    if ((__instance._hub.characterClassManager.CurClass != RoleType.Scp106 || (PlayerMovementSync._sphereHits[i].gameObject.layer != 27 && PlayerMovementSync._sphereHits[i].gameObject.layer != 14)) && (PlayerMovementSync._sphereHits[i].gameObject.layer != 27 || (scp = (__instance._hub.scpsController.CurrentScp as PlayableScps.Scp096)) == null || !scp.Enraged))
                        continue;

                    if (PlayerMovementSync._sphereHits[i].gameObject.layer == 27)
                    {
                        Door componentInParent = PlayerMovementSync._sphereHits[i].GetComponentInParent<Door>();
                        if (componentInParent != null && componentInParent.curCooldown > 0f && !componentInParent.isOpen)
                            continue;
                    }

                    __result = true;
                }

                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Api: AntiCheatPatch failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSync),nameof(PlayerMovementSync.AnticheatRaycast))]
    internal static class AntiCheatPatch2
    {
        private static void Prefix(PlayerMovementSync __instance,ref Vector3 offset)
        {
            var player = __instance.GetPlayer();
            offset.y *= player.Scale.y;
        }
    }
}
