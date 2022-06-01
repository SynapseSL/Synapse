using HarmonyLib;
using PlayerStatsSystem;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(HealthStat), "get_MaxValue")]
    internal static class HealthPatch
    {
        [HarmonyPrefix]
        private static bool GetMaxHealth(HealthStat __instance, out float __result)
        {
            __result = __instance.GetPlayer().MaxHealth;
            return false;
        }
    }

    [HarmonyPatch(typeof(AhpStat), nameof(AhpStat.ServerAddProcess), new[] { typeof(float) })]
    internal static class AhpPatch
    {
        [HarmonyPrefix]
        private static bool ServerAddProcess(AhpStat __instance, float amount, out AhpStat.AhpProcess __result)
        {
            __result = __instance.ServerAddProcess(amount, __instance.GetPlayer().MaxArtificialHealth, 1.2f, 0.7f, 0f, false);
            return false;
        }
    }
}
