using System;
using HarmonyLib;
using PlayableScps;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp096
{
    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.ParseVisionInformation))]
    static class Scp096LookPatch
    {
        public static bool Prefix(PlayableScps.Scp096 __instance, VisionInformation info)
        {
            try
            {
                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(info.Source.GetPlayer(), __instance.GetPlayer(), __instance.PlayerState, out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}");
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
                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(info.RHub.GetPlayer(), __instance.GetPlayer(), __instance.PlayerState, out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}");
                return true;
            }
        }
    }
}