using System;
using HarmonyLib;
using Neuron.Core.Logging;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class MiscPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public static void ReloadName()
    {
        if(!Synapse.Get<SynapseConfigService>().HostingConfiguration.NameTracking) return;

        ServerConsole._serverName +=
            $" <color=#00000000><size=1>Synapse {Synapse.GetVersion()}</size></color>";
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthStat), "get_MaxValue")]
    private static bool GetMaxHealth(HealthStat __instance, out float __result)
    {
        __result = __instance.GetPlayer().MaxHealth;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AhpStat), nameof(AhpStat.ServerAddProcess), typeof(float))]
    private static bool ServerAddProcess(AhpStat __instance, float amount, out AhpStat.AhpProcess __result)
    {
        var player = __instance.GetPlayer();
        __result = __instance.ServerAddProcess(amount, player.MaxArtificialHealth, 1.2f, 0.7f, 0f, false);
        return false;
    }
}