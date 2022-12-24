namespace Synapse3.SynapseModule.Patching.Patches;

#if DEV
using System;
using HarmonyLib;
using Neuron.Core.Meta;

[Automatic]
[SynapsePatch("No ServerList",PatchType.Dev)]
public static class DevPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.RunServer))]
    public static bool OnVerification()
    {
        ServerConsole.AddLog("Server WON'T be visible on the public list due to usage of a Synapse Dev Version. This Version is only intended to be used for developers and not verified servers!",ConsoleColor.DarkRed);
        return false;
    }
}
#endif