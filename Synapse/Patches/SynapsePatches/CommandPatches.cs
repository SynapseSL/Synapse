using Harmony;
using RemoteAdmin;
using System;
using Logger = Synapse.Api.Logger;

// ReSharper disable All
namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(GameCore.Console),nameof(GameCore.Console.TypeCommand))]
    internal static class ServerCommandPatch
    {
        private static bool Prefix(string cmd, CommandSender sender = null)
        {
            var args = cmd.Split(' ');

            if (cmd.StartsWith(".") || cmd.StartsWith("/") || cmd.StartsWith("@"))
                return true;

            if(SynapseController.CommandHandlers.ServerConsoleHandler.TryGetCommand(args[0],out var command))
            {
                try
                {
                    var flag = command.Execute(args.Segment(1), Server.Get.Host, Command.Platform.ServerConsole, out var text);
                    Logger.Get.Send(text, flag ? ConsoleColor.Green : ConsoleColor.Red);
                }
                catch(Exception e)
                {
                    Logger.Get.Error($"Synapse-Commands: Command Execution of failed!!\n{e}");
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(QueryProcessor),nameof(QueryProcessor.ProcessGameConsoleQuery))]
    internal static class ClientCommandPatch
    {
        private static bool Prefix(QueryProcessor __instance,string query,bool encrypted)
        {
            var player = __instance._sender.GetPlayer();
            var args = query.Split(' ');
            Logger.Get.Info(query);
            if(SynapseController.CommandHandlers.ClientCommandHandler.TryGetCommand(args[0],out var command))
            {
                try
                {
                    var flag = command.Execute(args.Segment(1), player, Command.Platform.ClientConsole, out var text);
                    player.SendConsoleMessage(text, flag ? "gray" : "red");
                }
                catch(Exception e)
                {
                    Logger.Get.Error($"Synapse-Commands: Command Execution of failed!!\n{e}");
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CommandProcessor),nameof(CommandProcessor.ProcessQuery))]
    internal static class RACommandPatch
    {
        private static bool Prefix(string q, CommandSender sender)
        {
            var player = sender.GetPlayer();
            var args = q.Split(' ');

            if (q.StartsWith("@"))
                return true;

            if(SynapseController.CommandHandlers.RemoteAdminHandler.TryGetCommand(args[0],out var command))
            {
                try
                {
                    var flag = command.Execute(args.Segment(1), player, Command.Platform.RemoteAdmin, out var text);
                    player.SendRAConsoleMessage(text, flag);
                }
                catch(Exception e)
                {
                    Logger.Get.Error($"Synapse-Commands: Command Execution of failed!!\n{e}");
                }
                return false;
            }

            return true;
        }
    }
}
