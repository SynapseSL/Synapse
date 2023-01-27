﻿using HarmonyLib;
using Mirror;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("ServerName", PatchType.Misc)]
public static class ServerNamePatch
{
    private static readonly SynapseConfigService ConfigService;
    static ServerNamePatch() => ConfigService = Synapse.Get<SynapseConfigService>();
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ServerConsole),nameof(ServerConsole.ReloadServerName))]
    public static void ReloadName()
    {
        if(!ConfigService.HostingConfiguration.NameTracking) return;

        ServerConsole._serverName +=
            $" <color=#00000000><size=1>Synapse {Synapse.GetVersion()}</size></color>";
    }
}
