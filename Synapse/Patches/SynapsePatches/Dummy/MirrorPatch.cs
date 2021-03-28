using Mirror;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
    internal static class MirrorPatch
    {
        private static bool Prefix(NetworkBehaviour __instance)
        {
            var player = __instance.GetPlayer();
            if (player != null && player.IsDummy) return false;
            return true;
        }
    }
}
