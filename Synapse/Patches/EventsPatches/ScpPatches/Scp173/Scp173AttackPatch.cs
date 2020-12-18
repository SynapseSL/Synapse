using System;
using HarmonyLib;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp173
{
    [HarmonyPatch(typeof(Scp173PlayerScript),nameof(Scp173PlayerScript.CallCmdHurtPlayer))]
    internal static class Scp173AttackPatch
    {
        private static bool Prefix(Scp173PlayerScript __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || target == null) return false;

                var player = target.GetPlayer();
                var scp = __instance.GetPlayer();

                if (scp.RoleType != RoleType.Scp173 || !__instance.CanMove(true) || Vector3.Distance(scp.Position, player.Position) >= 3f + __instance.boost_teleportDistance.Evaluate(__instance._ps.GetHealthPercent()))
                    return false;

                if(!scp.WeaponManager.GetShootPermission(player.ClassManager))
                    return false;

                SynapseController.Server.Events.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp173_Snap, out var allow);

                if (!allow) return false;

                __instance.RpcSyncAudio();
                scp.ClassManager.RpcPlaceBlood(player.transform.position, 0, 2.2f);
                player.Hurt(999990, DamageTypes.Scp173, scp);
                __instance.TargetHitMarker(scp.Connection);
                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp173) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
