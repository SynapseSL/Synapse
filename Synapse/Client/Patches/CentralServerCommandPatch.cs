using System;
using HarmonyLib;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(ServerConsole),nameof(ServerConsole.RunCentralServerCommand))]
    internal static class CentralServerCommandPatch
    {
        private static bool Prefix()
        {
            if (!ClientManager.IsSynapseClientEnabled) return true;
            Synapse.Api.Logger.Get.Info("The Synapse Central Server does not support Central Server Commands");
            return false;
        }
    }
}
