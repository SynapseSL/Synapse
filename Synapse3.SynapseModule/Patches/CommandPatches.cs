using System;
using HarmonyLib;
using Neuron.Core.Logging;
using Neuron.Modules.Commands.Command;
using RemoteAdmin;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Player;
using Console = GameCore.Console;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class CommandPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Console), nameof(Console.TypeCommand))]
    public static bool OnServerConsoleCommand(string cmd)
    {
        try
        {
            if (cmd.StartsWith(".") || cmd.StartsWith("/") || cmd.StartsWith("@") || cmd.StartsWith("!"))
                return true;

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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    public static bool OnPlayerConsoleCommand(QueryProcessor __instance, string query)
    {
        try
        {
            var player = __instance._sender.GetSynapsePlayer();
            if (player == null) return true;

            var result = Synapse.Get<SynapseCommandService>().PlayerConsole
                .Invoke(SynapseContext.Of(query, player, CommandPlatform.PlayerConsole));
            
            if (result.StatusCodeInt == 0) return true;

            var color = "white";
            switch (result.StatusCode)
            {
                case CommandStatusCode.Ok:
                    color = "gray";
                    break;
                    
                case CommandStatusCode.Error:
                    color = "red";
                    break;
                
                case CommandStatusCode.Forbidden:
                    color = "darkred";
                    break;
                
                case CommandStatusCode.BadSyntax:
                    color = "yellow";
                    break;
                    
                case CommandStatusCode.NotFound:
                    color = "green";
                    break;
            }

            player.SendConsoleMessage(result.Response, color);

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error($"S3 Commands: PlayerConsole command failed:\n{ex}");
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static bool OnRemoteAdminCommand(string q, CommandSender sender)
    {
        try
        {
            var player = sender.GetSynapsePlayer();
            
            //@ is used for AdminChat and $ for Communication like getting the playerList
            if (q.StartsWith("@") || q.StartsWith("$"))
                return true;

            var result = Synapse.Get<SynapseCommandService>().RemoteAdmin
                .Invoke(SynapseContext.Of(q, player, CommandPlatform.RemoteAdmin));

            if (result.StatusCodeInt == 0) return true;

            player.SendRaConsoleMessage(result.Response, result.StatusCodeInt == (int)CommandStatusCode.Ok);

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error($"S3 Commands: RemoteAdmin command failed:\n{ex}");
            return true;
        }
    }
}