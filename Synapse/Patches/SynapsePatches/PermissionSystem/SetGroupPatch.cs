using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.PermissionSystem
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetGroup))]
    internal static class PermissionPatch
    {
        [HarmonyPrefix]
        private static bool SetGroup() => false;
    }
}
