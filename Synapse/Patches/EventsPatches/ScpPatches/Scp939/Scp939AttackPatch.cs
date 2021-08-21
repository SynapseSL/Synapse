using System;
using HarmonyLib;
using Mirror;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp939
{
    [HarmonyPatch(typeof(Scp939PlayerScript),nameof(Scp939PlayerScript.UserCode_CmdShoot))]
    internal static class Scp939AttackPatch
    {
        private static bool Prefix(Scp939PlayerScript __instance, GameObject target)
        {
            try
            {
                if (target == null || !__instance.iAm939 || __instance.cooldown > 0f) return false;
                var scp = __instance.GetPlayer();
                var player = target.GetPlayer();
                if (Vector3.Distance(player.Position, scp.Position) >= __instance.attackDistance * 1.2f) return false;

                if (!SynapseExtensions.GetHarmPermission(scp, player)) return false;

                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp939_Bite, out var allow);

                if (!allow) return false;

                __instance.cooldown = 1f;
                player.Hurt(50, DamageTypes.Scp939, scp);
                scp.ClassManager.RpcPlaceBlood(player.Position, 0, 2f);
                player.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Amnesia>(3f, true);
                __instance.RpcShoot();

                return false;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp939) failed!!\n{e}");
                return true;
            }
            /*
            try
            {
                var scp = __instance.GetPlayer();

                var victim = msg.Victim;

                if (victim == null || __instance._currentBiteCooldown > 0f || Vector3.Distance(victim.transform.position, __instance.Hub.transform.position) >= 2.4f)
                    return false;

                var window = victim.GetComponent<BreakableWindow>();
                var player = victim.GetPlayer();

                if(window != null || player != null)
                {
                    __instance._currentBiteCooldown = 1f;

                    scp.Connection.Send(new PlayableScps.Messages.ScpHitmarkerMessage(1.5f), 0);
                    NetworkServer.SendToAll(default(PlayableScps.Messages.Scp939OnHitMessage));

                    if(window != null)
                    {
                        window.ServerDamageWindow(50f);
                        return false;
                    }

                    if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, player.Hub)) return false;

                    ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp939_Bite, out var allow);

                    if (!allow) return false;

                    player.Hurt(50, DamageTypes.Scp939, scp);
                    scp.ClassManager.RpcPlaceBlood(victim.transform.position, 0, 2f);
                    player.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Amnesia>(3f, true);
                }

                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp939) failed!!\n{e}");
                return true;
            }
            */
        }
    }
}
