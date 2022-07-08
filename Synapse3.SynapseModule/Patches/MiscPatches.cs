using HarmonyLib;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using UnityEngine;

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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OverridePosition))]
    private static bool OnSetPosition(PlayerMovementSync __instance, Vector3 pos,
        PlayerMovementSync.PlayerRotation? rot, bool forceGround)
    {
        try
        {
            if (forceGround && Physics.Raycast(pos, Vector3.down, out var hit, 100f, __instance.CollidableSurfaces))
            {
                pos = hit.point + Vector3.up * 1.23f * __instance.transform.localScale.y;
            }

            __instance.ForcePosition(pos);

            if (rot != null)
                __instance.ForceRotation(rot.Value);
        }
        catch{ }
        return false;
    }
}