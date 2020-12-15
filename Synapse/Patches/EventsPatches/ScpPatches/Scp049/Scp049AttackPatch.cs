using System;
using UnityEngine;
using HarmonyLib;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp049
{
    [HarmonyPatch(typeof(PlayableScps.Scp049),nameof(PlayableScps.Scp049.BodyCmd_ByteAndGameObject))]
    internal static class Scp049AttackPatch
    {
        private static bool Prefix(PlayableScps.Scp049 __instance, byte num, GameObject go)
        {
            if (num != 0) return true;

            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || go == null) return false;

                var scp = __instance.GetPlayer();
                var player = go.GetPlayer();

                if (player == null || Vector3.Distance(scp.Position, player.Position) >= PlayableScps.Scp049.AttackDistance * 1.25f || !scp.WeaponManager.GetShootPermission(player.ClassManager))
                    return false;

                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp049_Touch, out var allow);
                if (!allow) return false;

                player.Hurt(4949, DamageTypes.Scp049, scp);
                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Sent 'death time' RPC", MessageImportance.LessImportant, false);
                scp.Hub.scpsController.RpcTransmit_Byte(0);
                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp049) failed!!\n{e}");
                return true;
            }
        }
    }
}
