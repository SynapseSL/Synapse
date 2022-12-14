using HarmonyLib;
using PlayableScps;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Config;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches;

[Patches]
[HarmonyPatch]
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
    public static bool GetMaxHealth(HealthStat __instance, out float __result)
    {
        __result = __instance.GetSynapsePlayer().MaxHealth;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AhpStat), nameof(AhpStat.ServerAddProcess), typeof(float))]
    public static bool ServerAddProcess(AhpStat __instance, float amount, out AhpStat.AhpProcess __result)
    {
        var player = __instance.GetSynapsePlayer();
        __result = __instance.ServerAddProcess(amount, player.MaxArtificialHealth, player.DecayArtificialHealth, 0.7f,
            0f, false);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OverridePosition))]
    public static bool OnSetPosition(PlayerMovementSync __instance, Vector3 pos,
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
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Scp096), "CurMaxShield", MethodType.Getter)]
    public static void GetMaxShield(Scp096 __instance, ref float __result)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if(player == null) return;

            __result = player.ScpController.Scp096.MaxShield;
        }
        catch{ }
    }
}