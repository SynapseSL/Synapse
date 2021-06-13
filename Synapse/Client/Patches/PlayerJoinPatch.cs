using HarmonyLib;
using Synapse.Client.Packets;
using Synapse.Network;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.UpdateNickname))]
    internal static class PlayerJoinPatch {
        private static void Prefix(NicknameSync __instance, ref string n)
        {
            var player = __instance.GetPlayer();
            if (ClientManager.isSynapseClientEnabled)
            {
                ClientPipeline.invoke(player, PipelinePacket.from(ConnectionSuccessfulPacket.ID, new byte[0]));
                ClientPipeline.invokeConnectionComplete(player);
            }
        }
    }
}