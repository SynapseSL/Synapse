using System;
using HarmonyLib;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS
[Automatic]
[SynapsePatch("ServerName", PatchType.Misc)]
public static class ServerNamePatch
{
    private static readonly SynapseConfigService ConfigService;
    static ServerNamePatch() => ConfigService = Synapse.Get<SynapseConfigService>();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public static void ReloadName()
    {
        try
        {
            if (!ConfigService.HostingConfiguration.NameTracking) return;

            ServerConsole._serverName +=
                $" <color=#00000000><size=1>Synapse {Synapse.GetVersion()}</size></color>";
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("ServerName Patch failed\n" + ex);
        }
    }
}
#endif