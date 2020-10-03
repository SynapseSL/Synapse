using System;
using Harmony;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches.Permission_System
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.RefreshPermissions))]
    internal static class PermissionPatch2
    {
        private static bool Prefix(ServerRoles __instance)
        {
            try
            {
                var player = __instance.GetPlayer();
                player.RefreshPermission();
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Permission: RefreshPermissionPatch failed!!\n{e}");
            }
            return false;
        }
    }
}
