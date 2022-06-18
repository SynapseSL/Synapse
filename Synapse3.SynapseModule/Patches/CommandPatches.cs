using System;
using HarmonyLib;
using Neuron.Core.Logging;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Player;

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
            var result = Synapse.Get<SynapseCommandService>().ServerConsole
                .Invoke(SynapseContext.Of(cmd, Synapse.Get<PlayerService>().Host, CommandPlatform.ServerConsole));

            if (result.StatusCodeInt == 0) return true;

            var color = ConsoleColor.White;
            switch (result.StatusCode)
            {
                case CommandStatusCode.Ok:
                    color = ConsoleColor.Cyan;
                    break;
                    
                case CommandStatusCode.Error:
                    color = ConsoleColor.Red;
                    break;
                
                case CommandStatusCode.Forbidden:
                    color = ConsoleColor.DarkRed;
                    break;
                
                case CommandStatusCode.BadSyntax:
                    color = ConsoleColor.Yellow;
                    break;
                    
                case CommandStatusCode.NotFound:
                    color = ConsoleColor.DarkGreen;
                    break;
            }

            ServerConsole.AddLog(result.Response, color);
            
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error($"S3 Commands: ServerConsole command failed:\n{ex}");
            return true;
        }
    }
}