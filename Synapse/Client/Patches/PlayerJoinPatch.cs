using System.Linq;
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
                var mods = SynapseController.PluginLoader.Plugins.SelectMany(x => x.ActivatedClientMods).ToArray();
                ClientPipeline.Invoke(player, ConnectionSuccessfulPacket.Encode(mods));
                ClientPipeline.InvokeConnectionComplete(player);
            }
        }
    }
}