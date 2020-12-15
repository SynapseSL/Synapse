using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Mirror;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp096
{
    [HarmonyPatch(typeof(PlayableScps.Scp096),nameof(PlayableScps.Scp096.AttackTriggerPhysics))]
    internal static class Scp096AttackPatch
    {
        private static bool Prefix(PlayableScps.Scp096 __instance,out IEnumerator<float> __result)
        {
            __result = NewAttackTriggerPhysics(__instance);
            return false;
        }

        private static IEnumerator<float> NewAttackTriggerPhysics(PlayableScps.Scp096 __instance)
        {
            var scp = __instance.GetPlayer();
            var alreadyHit = new HashSet<GameObject>();
            int armAttack = __instance._leftAttack ? 1 : -1;
            do
            {
                var b = scp.CameraReference.TransformDirection(0.25f * armAttack, 0f, 1.3f);
                int num = Physics.OverlapSphereNonAlloc(scp.CameraReference.position + b, 1f, PlayableScps.Scp096._cachedAttackSwingColliders, PlayableScps.Scp096._attackHitMask);
                var num2 = 0f;
                for (int i = 0; i < num; i++)
                {
                    var collider = PlayableScps.Scp096._cachedAttackSwingColliders[i];
                    var comp = collider.GetComponentInParent<Door>();
                    if (comp != null)
                    {
                        comp.DestroyDoor(true);
                        if (comp.destroyed && num2 < 1f)
                            num2 = 1f;
                    }
                    else
                    {
                        var comp2 = collider.GetComponentInParent<BreakableWindow>();
                        if (comp2 != null)
                        {
                            comp2.ServerDamageWindow(500f);
                            if (num2 < 0.5f)
                                num2 = 0.5f;
                        }
                        else
                        {
                            var player = collider.GetComponentInParent<Synapse.Api.Player>();

                            if (player == null || player == scp) continue;
                            if (!alreadyHit.Add(player.gameObject) ||
                                Physics.Linecast(scp.transform.position, player.transform.position, PlayableScps.Scp096._solidObjectMask)) continue;

                            if (!scp.WeaponManager.GetShootPermission(player.ClassManager)) continue;

                            var allow = true;

                            try
                            {
                                ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp096_Tear, out allow);
                            }
                            catch (Exception e)
                            {
                                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096) failed!!\n{e}");
                            }

                            if (allow)
                            {
                                num2 = 1.35f;
                                player.Hurt(9696, DamageTypes.Scp096, scp);
                                __instance._targets.Remove(player.Hub);
                                NetworkServer.SendToAll(default(PlayableScps.Messages.Scp096OnKillMessage), 0);
                            }
                        }
                    }
                }

                if (num > 0f)
                    NetworkServer.SendToClientOfPlayer(scp.NetworkIdentity, new PlayableScps.Messages.Scp096HitmarkerMessage(num2));

                yield return MEC.Timing.WaitForOneFrame;
            }
            while (__instance._attackDuration >= 0.099999994f);     
            yield break;
        }
    }
}
