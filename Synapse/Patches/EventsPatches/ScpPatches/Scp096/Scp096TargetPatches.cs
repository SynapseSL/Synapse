using System;
using HarmonyLib;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp096
{
    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.AddTarget))]
    static class Scp096LookPatch
    {
        public static bool Prefix(PlayableScps.Scp096 __instance, GameObject target)
        {
            try
            {
                if (target == null) return true;

                var player = target.GetPlayer();

                if (player.Invisible)
                    return false;

                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(player, __instance.GetPlayer(), __instance.PlayerState, out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.OnDamage))]
    static class Scp096ShootPatch
    {
        public static bool Prefix(PlayableScps.Scp096 __instance, PlayerStats.HitInfo info)
        {
            try
            {
                if (info == null || info.RHub == null) return false;

                var player = info.RHub.GetPlayer();

                if (player.Invisible)
                    return false;

                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(player, __instance.GetPlayer(), __instance.PlayerState, out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}");
                return true;
            }
        }
    }
}