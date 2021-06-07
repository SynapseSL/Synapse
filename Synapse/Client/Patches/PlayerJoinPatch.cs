using HarmonyLib;
using Synapse.Network;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.UpdateNickname))]
    internal static class PlayerJoinPatch {
        private static void Prefix(NicknameSync __instance)
        {
            var player = __instance.GetPlayer();
            if (ClientManager.IsSynapseClientEnabled)
            {
                ClientPipeline.Invoke(player, PipelinePacket.From(0, "Login successful"));
            }
        }
    }
}