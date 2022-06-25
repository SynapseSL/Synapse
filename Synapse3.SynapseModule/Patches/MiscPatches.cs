using HarmonyLib;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class MiscPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public static void ReloadName()
    {
        if(!Synapse.Get<SynapseConfigService>().HostingConfiguration.NameTracking) return;

        ServerConsole._serverName +=
            $" <color=#00000000><size=1>Synapse {Synapse.GetVersion()}</size></color>";
    }
}