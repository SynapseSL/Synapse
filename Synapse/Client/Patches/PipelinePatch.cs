using HarmonyLib;
using Synapse.Api;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(GameConsoleTransmission), nameof(GameConsoleTransmission.CallCmdCommandToServer))]
    internal static class PipelinePatch
    {
        private static bool Prefix(GameConsoleTransmission __instance, byte[] data, bool encrypted)
        {
            if (!encrypted && SynapseController.ClientManager.IsSynapseClientEnabled)
            {
                if (DataUtils.IsData(data))
                {
                    ClientPipeline.Receive(__instance.gameObject.GetPlayer(), DataUtils.Unpack(data));
                    return false;
                }
            }

            return true;
        }
    }
}