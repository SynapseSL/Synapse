using System;
using HarmonyLib;
using Mirror;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp939
{
    [HarmonyPatch(typeof(PlayableScps.Scp939),nameof(PlayableScps.Scp939.ServerAttack))]
    internal static class Scp939AttackPatch
    {
        [HarmonyPrefix]
        private static bool OnBite(PlayableScps.Scp939 __instance, PlayableScps.Messages.Scp939AttackMessage msg)
        {
            try
            {
                var scp = __instance.GetPlayer();

                if (msg.Victim != null && msg.Victim.TryGetComponent<BreakableWindow>(out var window))
                {
                    __instance._currentBiteCooldown = 1f;
                    scp.Connection.Send(new PlayableScps.Messages.ScpHitmarkerMessage(1.5f));
                    NetworkServer.SendToAll(new PlayableScps.Messages.Scp939OnHitMessage(scp.PlayerId));
                    window.Damage(50f, null, new Footprinting.Footprint(scp.Hub), Vector3.zero);
                    return false;
                }

                var player = msg.Victim?.GetPlayer();
                if (player == null) return false;

                if (!SynapseExtensions.GetHarmPermission(scp, player)) return false;

                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp939_Bite, out var allow);

                if (!allow) return false;

                __instance._currentBiteCooldown = 1f;
                scp.Connection.Send(new PlayableScps.Messages.ScpHitmarkerMessage(1.5f));
                NetworkServer.SendToAll(new PlayableScps.Messages.Scp939OnHitMessage(scp.PlayerId));
                player.Hurt(50, DamageTypes.Scp939, scp);
                scp.ClassManager.RpcPlaceBlood(player.Position, 0, 2f);
                player.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Amnesia>(3f, true);
                return false;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp939) failed!!\n{e}");
                return true;
            }
        }
    }
}
