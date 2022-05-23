using Mirror;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
    internal static class MirrorPatch
    {
        [HarmonyPrefix]
        private static bool TargetRPC(NetworkBehaviour __instance)
        {
            var player = __instance.GetPlayer();
            if (player is not null && player.IsDummy) return false;
            return true;
        }
    }
}