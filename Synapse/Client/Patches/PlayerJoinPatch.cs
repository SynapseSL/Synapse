using HarmonyLib;
using Synapse.Network;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.UpdateNickname))]
    internal static class PlayerJoinPatch {
        private static void Prefix(NicknameSync __instance, ref string n)
        {
            var player = __instance.GetPlayer();
            if (ClientManager.IsSynapseClientEnabled)
            {
                ClientPipeline.invoke(player, PipelinePacket.from(0, "Login successful"));
            }
        }
    }
}