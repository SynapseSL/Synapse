using System;
using HarmonyLib;
using Neuron.Core.Logging;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class CommandPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameCore.Console), nameof(GameCore.Console.TypeCommand))]
    private static bool OnConsoleCommand(string cmd)
    {
        try
        {
            return true;
        }
        catch (Exception ex)
        {
            
            return true;
        }
    }
}