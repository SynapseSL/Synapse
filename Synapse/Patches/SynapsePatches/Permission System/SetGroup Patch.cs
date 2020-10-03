using System;
using Harmony;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
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
