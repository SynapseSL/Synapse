using HarmonyLib;
using Synapse.Api;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(GameConsoleTransmission), nameof(GameConsoleTransmission.CallCmdCommandToServer))]
    internal static class PipelinePatch
    {
        private static bool Prefix(GameConsoleTransmission __instance, byte[] data, bool encrypted)
        {
            if (!encrypted && ClientManager.IsSynapseClientEnabled)
            {
                if (DataUtils.isData(data))
                {
                    var unpacked = DataUtils.unpack(data);
                    ClientPipeline.receive(__instance.gameObject.GetPlayer(), DataUtils.unpack(data));
                    return false;
                }
            }

            return true;
        }
    }
}