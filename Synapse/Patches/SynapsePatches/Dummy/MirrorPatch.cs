using HarmonyLib;
using Mirror;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
    internal static class MirrorPatch
    {
        [HarmonyPrefix]
        private static bool TargetRPC(NetworkBehaviour __instance)
        {
            var player = __instance.GetPlayer();
            return player == null || !player.IsDummy;
        }
    }
}
