using HarmonyLib;
using Synapse.Client.Packets;
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
                ClientPipeline.Invoke(player, PipelinePacket.From(ConnectionSuccessfulPacket.ID, new byte[0]));
                ClientPipeline.InvokeConnectionComplete(player);
            }
        }
    }
}