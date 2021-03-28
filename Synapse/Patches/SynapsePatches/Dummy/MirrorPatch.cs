using Mirror;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
    internal static class MirrorPatch
    {
        private static bool Prefix(NetworkConnection conn) => conn != null;
    }
}
