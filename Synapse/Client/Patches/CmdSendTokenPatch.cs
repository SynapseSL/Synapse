using HarmonyLib;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.CallCmdSendToken))]
    internal static class CmdSendTokenPatch
    {
        private static bool Prefix()
        {
            if (SynapseController.ClientManager.IsSynapseClientEnabled) return false;
            return true;
        }
    }
}
