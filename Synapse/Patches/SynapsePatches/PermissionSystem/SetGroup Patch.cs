using Harmony;

namespace Synapse.Patches.SynapsePatches.PermissionSystem
{
    [HarmonyPatch(typeof(ServerRoles),nameof(ServerRoles.SetGroup))]
    internal static class PermissionPatch
    {
        private static bool Prefix(ServerRoles __instance)
        {
            return false;
        }
    }
}
