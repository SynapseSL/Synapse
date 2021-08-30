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
        private static bool Scp939Attack(PlayableScps.Scp939 __instance, PlayableScps.Messages.Scp939AttackMessage msg)
        {
            try
            {
                var scp = Server.Get.GetPlayer(msg.PlayerID);

                if (scp == null || msg.Victim == null || __instance._currentBiteCooldown > 0f || Vector3.Distance(scp.Position, msg.Victim.transform.position) >= 2.87)
                    return false;

                if(msg.Victim.TryGetComponent<BreakableWindow>(out var window))
                {
                    window.Damage(50f, null, new Footprinting.Footprint(scp.Hub), Vector3.zero);
                    __instance._currentBiteCooldown = 1f;

                    scp.Connection.Send(new PlayableScps.Messages.ScpHitmarkerMessage(1.5f));
                    NetworkServer.SendToAll(new PlayableScps.Messages.Scp939OnHitMessage(scp.PlayerId));
                }
                else
                {
                    __instance._currentBiteCooldown = 1f;

                    scp.Connection.Send(new PlayableScps.Messages.ScpHitmarkerMessage(1.5f));
                    NetworkServer.SendToAll(new PlayableScps.Messages.Scp939OnHitMessage(scp.PlayerId));
                    var target = msg.Victim.GetPlayer();

                    if (!SynapseExtensions.GetHarmPermission(scp, target)) return false;

                    ev.Get.Scp.InvokeScpAttack(scp, target, Api.Enum.ScpAttackType.Scp939_Bite, out var allow);

                    if (!allow) return false;

                    scp.PlayerStats.HurtPlayer(new PlayerStats.HitInfo(50f, scp.Hub.LoggedNameFromRefHub(),
                      DamageTypes.Scp939, scp.PlayerId, false), msg.Victim, false, true);
                    scp.ClassManager.RpcPlaceBlood(target.Position, 0, 2f);

                    target.PlayerEffectsController.EnableEffect<CustomPlayerEffects.Amnesia>(3f, true);
                }

                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp939) failed!!\n{e}");
                return true;
            }
        }
    }
}
