using System;
using HarmonyLib;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.CallCmdSendToken))]
    internal static class CmdSendTokenPatch
    {
        private static bool Prefix()
        {
            if (ClientManager.isSynapseClientEnabled) return false;
            return true;
        }
    }
}
