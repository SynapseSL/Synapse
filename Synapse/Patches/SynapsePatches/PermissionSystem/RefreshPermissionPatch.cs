using HarmonyLib;
using Synapse.Api;
using System;

namespace Synapse.Patches.SynapsePatches.PermissionSystem
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.RefreshPermissions))]
    internal static class PermissionPatch2
    {
        [HarmonyPrefix]
        private static bool RefreshPermission(ServerRoles __instance, bool disp = false)
        {
            try
            {
                var player = __instance.GetPlayer();
                player.RefreshPermission(disp);
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Permission: RefreshPermissionPatch failed!!\n{e}");
            }

            return false;
        }
    }
}
