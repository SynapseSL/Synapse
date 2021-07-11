using System.Linq;
using HarmonyLib;
using Synapse.Client.Packets;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.UpdateNickname))]
    internal static class PlayerJoinPatch {
        private static void Prefix(NicknameSync __instance)
        {
            var player = __instance.GetPlayer();
            if (SynapseController.ClientManager.IsSynapseClientEnabled)
            {
                var mods = SynapseController.PluginLoader.Plugins.SelectMany(x => x.ActivatedClientMods).Distinct().ToArray();
                ClientPipeline.Invoke(player, ConnectionSuccessfulPacket.Encode(mods));
                ClientPipeline.InvokeConnectionComplete(player);
            }
        }
    }
}