using HarmonyLib;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    internal static class ServerNamePatch
    {
        [HarmonyPostfix]
        private static void ReloadName()
        {
            if (!Server.Get.Configs.SynapseConfiguration.NameTracking) return;

            ServerConsole._serverName += $" <color=#00000000><size=1>Synapse {SynapseVersion.GetVersionName()}</size></color>";
        }
    }
}
