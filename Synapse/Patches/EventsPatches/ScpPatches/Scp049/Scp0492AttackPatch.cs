using System;
using HarmonyLib;
using PlayerStatsSystem;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp049
{
    [HarmonyPatch(typeof(Scp049_2PlayerScript), nameof(Scp049_2PlayerScript.UserCode_CmdHurtPlayer))]
    internal static class Scp0492AttackPatch
    {
        [HarmonyPrefix]
        private static bool HurtPlayer(Scp049_2PlayerScript __instance, GameObject plyObj)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || plyObj == null) return false;

                var scp = __instance.GetPlayer();
                var player = plyObj?.GetPlayer();

                if (player is null) return false;

                if (!__instance.iAm049_2 || Vector3.Distance(scp.Position, player.Position) > __instance.distance * 1.5f) return false;

                if (!SynapseExtensions.GetHarmPermission(scp, player)) return false;

                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp0492_Scratch, out var allow);

                if (!allow) return false;

                player.PlayerStats.DealDamage(new ScpDamageHandler(scp.Hub, __instance.damage, DeathTranslations.Zombie));
                Hitmarker.SendHitmarker(scp.Connection, 1f);
                scp.ClassManager.RpcPlaceBlood(player.Position, 0, player.RoleType is RoleType.Spectator ? 1.3f : 0.5f);
                return false;
            }
            catch (Exception e)
            {
                Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp049-2) failed!!\n{e}");
                return true;
            }
        }
    }
}