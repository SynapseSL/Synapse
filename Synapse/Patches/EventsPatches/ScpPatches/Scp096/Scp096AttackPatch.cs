using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using PlayerStatsSystem;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp096
{
    [HarmonyPatch(typeof(PlayableScps.Scp096),nameof(PlayableScps.Scp096.ServerHitObject))]
    internal static class Scp096AttackPatch
    {
        [HarmonyPrefix]
        private static bool HitObject(PlayableScps.Scp096 __instance, GameObject target, out bool __result)
        {
            try
            {
                __result = false;

                if (target.TryGetComponent<BreakableWindow>(out var window))
                {
                    __result = window.Damage(500f, new Scp096DamageHandler(__instance, 500f, Scp096DamageHandler.AttackType.Slap), target.transform.position);
                    return false;
                }

                if(target.TryGetComponent<DoorVariant>(out var door) && (object)door is IDamageableDoor damageable && !door.IsConsideredOpen())
                {
                    __result = damageable.ServerDamage(250f, DoorDamageType.Scp096);
                    return false;
                }

                if(!ReferenceHub.TryGetHub(target, out var hub) || hub == null || hub == __instance.Hub || hub.characterClassManager.IsAnyScp())
                    return false;

                var scp = __instance.GetPlayer();
                var player = hub.GetPlayer();

                if (Physics.Linecast(scp.Position, player.Position, PlayableScps.Scp096._solidObjectMask)) 
                    return false;

                if (Vector3.Distance(scp.Position, player.Position) > 5f)
                    return false;

                if (!SynapseExtensions.GetHarmPermission(scp, player)) return false;
                try
                {
                    ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp096_Tear, out var allow);
                    if (!allow) return false;
                }
                catch (Exception e)
                {
                    Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096-Charge) failed!!\n{e}");
                }
                
                if (hub.playerStats.DealDamage(new Scp096DamageHandler(__instance, 9696f, Scp096DamageHandler.AttackType.Slap)))
                {
                    __instance._targets.Remove(hub);
                    NetworkServer.SendToAll(default(PlayableScps.Messages.Scp096OnKillMessage), 0, false);
                }

                __result = true;
                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(HitObject) failed!!\n{e}");
                __result = false;
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096),nameof(PlayableScps.Scp096.ChargePlayer))]
    internal static class Scp096AttackPatch2
    {
        [HarmonyPrefix]
        private static bool ChargePlayer(PlayableScps.Scp096 __instance, ReferenceHub player)
        {
            try
            {   
                var scp = __instance.GetPlayer();
                var target = player.GetPlayer();
                if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, target.Hub)) return false;

                if (Physics.Linecast(scp.transform.position, player.transform.position, LayerMask.GetMask(new string[]
                {
                    "Default",
                    "Door",
                    "Glass"
                }))) return false;

                if (__instance._chargeHitTargets.Contains(player)) return false;


                try
                {
                    ev.Get.Scp.InvokeScpAttack(scp, target, Api.Enum.ScpAttackType.Scp096_Tear, out var allow);
                    if (!allow) return false;
                }
                catch (Exception e)
                {
                    Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096-Charge) failed!!\n{e}");
                }

                var flag = __instance._targets.Contains(player);
                var damage = flag ? 9696f : 40f;
                var flag2 = player.playerStats.DealDamage(new Scp096DamageHandler(__instance, damage, Scp096DamageHandler.AttackType.Charge));
                __instance._chargeHitTargets.Add(player);

                if (flag2)
                {
                    __instance._targets.Remove(player);

                    Hitmarker.SendHitmarker(__instance.Hub, 1.35f);
                    if (!__instance._chargeKilled)
                    {
                        __instance._chargeCooldownPenaltyAmount++;
                        __instance._chargeKilled = true;
                    }
                }
                if (flag) __instance.EndChargeNextFrame();

                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AttackEvent(Charge) failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.UpdatePry))]
    internal static class Scp096AttackPatch3
    {
        [HarmonyPrefix]
        private static bool Pry(PlayableScps.Scp096 __instance)
        {
            try
            {
                if (!__instance.PryingGate) return false;

                var num = Physics.OverlapSphereNonAlloc(__instance.Hub.playerMovementSync.RealModelPosition, 0.5f, PlayableScps.Scp096._sphereHits,LayerMask.GetMask(new string[]
                {
                "Hitbox"
                }));

                if (num <= 0) return false;

                for (int i = 0; i < num; i++)
                {
                    ReferenceHub componentInParent = PlayableScps.Scp096._sphereHits[i].gameObject.GetComponentInParent<ReferenceHub>();

                    if (componentInParent == null || componentInParent == __instance.Hub) continue;

                    var scp = __instance.GetPlayer();
                    var target = componentInParent.GetPlayer();
                    if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, target.Hub)) continue;
                    try
                    {
                        ev.Get.Scp.InvokeScpAttack(scp, target, Api.Enum.ScpAttackType.Scp096_Tear, out var allow);
                        if (!allow) continue;
                    }
                    catch (Exception e)
                    {
                        Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096-Charge) failed!!\n{e}");
                    }

                    // if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9696f, null, DamageTypes.Scp096, __instance.Hub.playerId, false), componentInParent.gameObject, false, true))
                    if (componentInParent.playerStats.DealDamage(new Scp096DamageHandler(__instance, 9696f, Scp096DamageHandler.AttackType.GateKill)))
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
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AttackEvent(Pry) failed!!\n{e}");
                return true;
            }
        }
    }
}
