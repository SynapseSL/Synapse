using System;
using Harmony;
using PlayableScps;
using Synapse.Api;

namespace Synapse.Events.Patches.EventPatches.ScpPatches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ParseVisionInformation))]
    static class Scp096LookPatch
    {
        public static bool Prefix(Scp096 __instance, VisionInformation info)
        {
            try
            {
                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(info.Source.GetPlayer(), __instance.GetPlayer(), __instance.PlayerState, out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Info($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.OnDamage))]
    static class Scp096ShootPatch
    {
        public static bool Prefix(Scp096 __instance, PlayerStats.HitInfo info)
        {
            try
            {
                Server.Get.Events.Scp.Scp096.InvokeScpTargetEvent(info.RHub.GetPlayer(), __instance.GetPlayer(), __instance.PlayerState, out var allow);
                return allow;
            }
            catch (Exception e)
            {
                Logger.Get.Info($"Synapse-Event: Scp096AddTargetEvent failed!!\n{e}");
                return true;
            }
        }
    }
}