using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
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
            var alreadyDamagedDoors = HashSetPool<Interactables.Interobjects.DoorUtils.IDamageableDoor>.Shared.Rent();
            int armAttack = __instance._leftAttack ? 1 : -1;
            do
            {
                var b = scp.CameraReference.TransformDirection(0.25f * armAttack, 0f, 1.3f);
                int num = Physics.OverlapSphereNonAlloc(scp.CameraReference.position + b, 1f, PlayableScps.Scp096._cachedAttackSwingColliders, PlayableScps.Scp096._attackHitMask);
                var num2 = 0f;
                for (int i = 0; i < num; i++)
                {
                    var collider = PlayableScps.Scp096._cachedAttackSwingColliders[i];
                    var comp = collider.GetComponentInParent<Interactables.Interobjects.DoorUtils.DoorVariant>();
                    if (comp != null && (object)comp is Interactables.Interobjects.DoorUtils.IDamageableDoor damageableDoor)
                    {
                        if (alreadyDamagedDoors.Add(damageableDoor))
                        {
                            damageableDoor.ServerDamage(250f, Interactables.Interobjects.DoorUtils.DoorDamageType.Scp096);
                            if (num2 < 1f)
                                num2 = 1f;
                        }
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
                                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
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

    [HarmonyPatch(typeof(PlayableScps.Scp096),nameof(PlayableScps.Scp096.ChargePlayer))]
    internal static class Scp096AttackPatch2
    {
        private static bool Prefix(PlayableScps.Scp096 __instance, ReferenceHub player)
        {
            try
            {
                if (!NetworkServer.active) return false;
                
                var scp = __instance.GetPlayer();
                var target = player.GetPlayer();
                if (!scp.WeaponManager.GetShootPermission(player.characterClassManager)) return false;
                try
                {
                    ev.Get.Scp.InvokeScpAttack(scp, target, Api.Enum.ScpAttackType.Scp096_Tear, out var allow);
                    if (!allow) return false;
                }
                catch (Exception e)
                {
                    Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096-Charge) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                }

                if (Physics.Linecast(scp.transform.position, player.transform.position, LayerMask.GetMask(new string[]
                {
                    "Default",
                    "Door",
                    "Glass"
                })))
                    return false;
                var flag = __instance._targets.Contains(player);
                
                if(scp.PlayerStats.HurtPlayer(new PlayerStats.HitInfo(flag ? 9696f : 35f,player.LoggedNameFromRefHub(),DamageTypes.Scp096,
                    scp.PlayerId), player.gameObject, false, true))
                {
                    __instance._targets.Remove(player);
                    NetworkServer.SendToClientOfPlayer(scp.NetworkIdentity, new PlayableScps.Messages.Scp096HitmarkerMessage(1.35f));
                    NetworkServer.SendToAll(default(PlayableScps.Messages.Scp096OnKillMessage), 0);
                }
                if (flag)
                    __instance.EndChargeNextFrame();

                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AttackEvent(Charge) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.UpdatePry))]
    internal static class Scp096AttackPatch3
    {
        private static bool Prefix(PlayableScps.Scp096 __instance)
        {
            try
            {
                if (!__instance.PryingGate)
                {
                    return false;
                }
                Collider[] array = Physics.OverlapSphere(__instance.Hub.playerMovementSync.RealModelPosition, 0.5f, LayerMask.GetMask(new string[]
                {
                "Hitbox"
                }));
                if (array.Length == 0)
                {
                    return false;
                }
                Collider[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    ReferenceHub componentInParent = array2[i].gameObject.GetComponentInParent<ReferenceHub>();

                    var scp = __instance.GetPlayer();
                    var target = componentInParent.GetPlayer();
                    if (!scp.WeaponManager.GetShootPermission(target.ClassManager)) continue;
                    try
                    {
                        ev.Get.Scp.InvokeScpAttack(scp, target, Api.Enum.ScpAttackType.Scp096_Tear, out var allow);
                        if (!allow) continue;
                    }
                    catch (Exception e)
                    {
                        Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096-Charge) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                    }

                    if (!(componentInParent == null) && !(componentInParent == __instance.Hub) && !componentInParent.characterClassManager.IsAnyScp() && __instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9696f, __instance.Hub.LoggedNameFromRefHub(), DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId), componentInParent.gameObject, false, true))
                    {
                        if (__instance._targets.Contains(componentInParent))
                            __instance._targets.Remove(componentInParent);

                        NetworkServer.SendToAll(default(PlayableScps.Messages.Scp096OnKillMessage), 0);
                    }
                }
                if (Physics.Raycast(__instance.Hub.PlayerCameraReference.position, __instance.Hub.PlayerCameraReference.forward, 2f, LayerMask.GetMask(new string[]
                {
                "Default"
                })))
                    __instance.EndChargeNextFrame();

                return false;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AttackEvent(Pry) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
