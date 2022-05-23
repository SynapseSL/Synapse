using System;
using HarmonyLib;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches.Wrapper
{
    [HarmonyPatch(typeof(PlayableScps.Scp096), "CurMaxShield", MethodType.Getter)]
    internal static class Scp096MaxShieldPatch
    {
        [HarmonyPostfix]
        private static void GetMaxShield(PlayableScps.Scp096 __instance, ref float __result)
        {
            try
            {
                var ply = __instance?.GetPlayer();
                if (ply is null) return;
                __result = ply.Scp096Controller.MaxShield;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Wrapper: SCP-096 MaxShield failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}