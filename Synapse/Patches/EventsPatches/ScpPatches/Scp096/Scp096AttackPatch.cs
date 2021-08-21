using System;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using UnityEngine;
using ev = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp096
{
    [HarmonyPatch(typeof(PlayableScps.Scp096),nameof(PlayableScps.Scp096.ServerHitObject))]
    internal static class Scp096AttackPatch
    {
        private static bool Prefix(PlayableScps.Scp096 __instance, GameObject target, out bool __result)
        {
            try
            {
                __result = false;

                if (target.TryGetComponent<BreakableWindow>(out var window))
                {
                    window.ServerDamageWindow(500f);
                    __result = true;
                    return false;
                }

                if(target.TryGetComponent<DoorVariant>(out var door) && (object)door is IDamageableDoor damageable)
                {
                    damageable.ServerDamage(250f, DoorDamageType.Scp096);
                    __result = true;
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

                if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, player.Hub)) return false;
                try
                {
                    ev.Get.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp096_Tear, out var allow);
                    if (!allow) return false;
                }
                catch (Exception e)
                {
                    Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096-Charge) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                }

                if(__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9696f, __instance.Hub.LoggedNameFromRefHub(),
                    DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId, false), hub.gameObject, false, true))
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
        private static bool Prefix(PlayableScps.Scp096 __instance, ReferenceHub player)
        {
            try
            {
                if (!NetworkServer.active) return false;
                
                var scp = __instance.GetPlayer();
                var target = player.GetPlayer();
                if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, target.Hub)) return false;
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

                if (scp.PlayerStats.HurtPlayer(new PlayerStats.HitInfo(flag ? 9696f : 35f, player.LoggedNameFromRefHub(), DamageTypes.Scp096,
                    scp.PlayerId, false), player.gameObject, false, true))
                {
                    __instance._targets.Remove(player);
                    scp.NetworkIdentity.connectionToClient.Send(new PlayableScps.Messages.ScpHitmarkerMessage(1.35f));
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
                    if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, target.Hub)) continue;
                    try
                    {
                        ev.Get.Scp.InvokeScpAttack(scp, target, Api.Enum.ScpAttackType.Scp096_Tear, out var allow);
                        if (!allow) continue;
                    }
                    catch (Exception e)
                    {
                        Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp096-Charge) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                    }

                    if (!(componentInParent == null) && !(componentInParent == __instance.Hub) && !componentInParent.characterClassManager.IsAnyScp() && __instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9696f, __instance.Hub.LoggedNameFromRefHub(), DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId, false), componentInParent.gameObject, false, true))
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
