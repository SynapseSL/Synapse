using System;
using HarmonyLib;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp939
{
    [HarmonyPatch(typeof(Scp939PlayerScript),nameof(Scp939PlayerScript.CallCmdShoot))]
    internal static class Scp939AttackPatch
    {
        private static bool Prefix(Scp939PlayerScript __instance, GameObject target)
        {
            try
            {
                if (target == null || !__instance.iAm939 || __instance.cooldown > 0f ) return false;
                var scp = __instance.GetPlayer();
                var player = target.GetPlayer();
                if (Vector3.Distance(player.Position, scp.Position) >= __instance.attackDistance * 1.2f) return false;

                if (!scp.WeaponManager.GetShootPermission(player.ClassManager)) return false;

                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp939_Bite, out var allow);

                if (!allow) return false;

                __instance.cooldown = 1f;
                player.Hurt(65, DamageTypes.Scp939, scp);
                player.GiveEffect(Api.Enum.Effect.Amnesia, 1, 3f);
                __instance.RpcShoot();
                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp939) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
