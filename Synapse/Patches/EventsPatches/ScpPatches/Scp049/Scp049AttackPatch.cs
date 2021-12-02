using System;
using HarmonyLib;
using PlayerStatsSystem;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp049
{
    [HarmonyPatch(typeof(PlayableScps.Scp049),nameof(PlayableScps.Scp049.BodyCmd_ByteAndGameObject))]
    internal static class Scp049AttackPatch
    {
        [HarmonyPrefix]
        private static bool BodyInteract(PlayableScps.Scp049 __instance, byte num, GameObject go)
        {
            if (num != 0) return true;

            try
            {
                if (!__instance._interactRateLimit.CanExecute(true)) return false;
                if (go == null || __instance.RemainingServerKillCooldown > 0f) return false;

                var scp = __instance.GetPlayer();
                var player = go.GetPlayer();

                if (!SynapseExtensions.GetHarmPermission(scp, player)) return false;

                if (Vector3.Distance(scp.Position, player.Position) >= PlayableScps.Scp049.AttackDistance * 1.25f) 
                    return false;

                if (Physics.Linecast(scp.Position, player.Position, InventorySystem.Items.MicroHID.MicroHIDItem.WallMask)) return false;

                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp049_Touch, out var allow);
                if (!allow) return false;

                player.PlayerStats.DealDamage(new ScpDamageHandler(scp.Hub, 4949f, DeathTranslations.Scp049));
                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Sent 'death time' RPC", MessageImportance.LessImportant, false);
                scp.Hub.scpsController.RpcTransmit_Byte(0);
                __instance.RemainingServerKillCooldown = PlayableScps.Scp049.KillCooldown;
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
