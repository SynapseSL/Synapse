using System;
using HarmonyLib;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp049
{
    [HarmonyPatch(typeof(Scp049_2PlayerScript),nameof(Scp049_2PlayerScript.CallCmdHurtPlayer))]
    internal static class Scp0492AttackPatch
    {
        private static bool Prefix(Scp049_2PlayerScript __instance, GameObject plyObj)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || plyObj == null)  return false;

                var scp = __instance.GetPlayer();
                var player = plyObj.GetPlayer();

                if (player == null) return false;

                if (scp.RoleType != RoleType.Scp0492 || Vector3.Distance(scp.Position, player.Position) > __instance.distance * 1.5f) return false;

                if (!scp.WeaponManager.GetShootPermission(player.ClassManager)) return false;

                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp0492_Scratch, out var allow);

                if (!allow) return false;

                player.Hurt((int)__instance.damage, DamageTypes.Scp0492, scp);
                __instance.TargetHitMarker(scp.Connection);
                scp.ClassManager.RpcPlaceBlood(player.Position, 0, player.RoleType == RoleType.Spectator ? 1.3f : 0.5f);
                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp049-2) failed!!\n{e}");
                return true;
            }
        }
    }
}
