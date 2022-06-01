using HarmonyLib;
using System;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), new[] { typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool) })]
    internal static class ShootPermissionPatch
    {
        [HarmonyPrefix]
        private static bool CheckFF(out bool __result, ReferenceHub attacker, ReferenceHub victim, bool ignoreConfig = false)
        {
            try
            {
                __result = SynapseExtensions.GetHarmPermission(attacker.GetPlayer(), victim.GetPlayer(), ignoreConfig);
                return false;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-API: GetShootPermission  failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                __result = true;
                return true;
            }
        }
    }
}
