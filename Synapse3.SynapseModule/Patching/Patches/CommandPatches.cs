using System;
using HarmonyLib;
using Neuron.Core.Meta;
using Neuron.Modules.Commands.Command;
using RemoteAdmin;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;
using Utils.NonAllocLINQ;
using Console = GameCore.Console;

namespace Synapse3.SynapseModule.Patching.Patches;
#if !PATCHLESS
[Automatic]
[SynapsePatch("ServerConsole", PatchType.Command)]
internal static class ServerConsolePatch
{
    private static readonly SynapseCommandService _commandService;
    private static readonly PlayerService _playerService;
    static ServerConsolePatch()
    {
        _commandService = Synapse.Get<SynapseCommandService>();
        _playerService = Synapse.Get<PlayerService>();
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Console), nameof(Console.TypeCommand))]
    public static bool OnServerConsoleCommand(string cmd)
    {
        try
        {
            if (cmd.StartsWith(".") || cmd.StartsWith("/") || cmd.StartsWith("@") || cmd.StartsWith("!"))
                return true;
            var result =
                _commandService.ServerConsole.Invoke(SynapseContext.Of(cmd, _playerService.Host,
                    CommandPlatform.ServerConsole));
            
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
            SynapseLogger<Synapse>.Error($"S3 Commands: ServerConsole command failed:\n{ex}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("RemoteAdmin", PatchType.Command)]
public static class RemoteAdminPatch
{
    private static readonly SynapseCommandService _commandService;
    static RemoteAdminPatch() => _commandService = Synapse.Get<SynapseCommandService>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static bool OnRemoteAdminCommand(string q, CommandSender sender)
    {
        try
        {
            var player = sender.GetSynapsePlayer();
            if (player == null) return true;
            
            //@ is used for AdminChat and $ for Communication like getting the playerList
            if (q.StartsWith("@") || q.StartsWith("$"))
                return true;
            
            var result = _commandService.RemoteAdmin
                .Invoke(SynapseContext.Of(q, player, CommandPlatform.RemoteAdmin));
            if (result.StatusCodeInt == 0) return true;

            var info = result.Attachments.FirstOrDefault(x => x is RemoteAdminAttachment, null);

            if (info != null)
            {
                var raInfo = (RemoteAdminAttachment)info;
                player.SendRaConsoleMessage(result.Response, result.StatusCodeInt == (int)CommandStatusCode.Ok,
                    raInfo.DisplayCategory, raInfo.DisplayName);
            }
            else
            {
                player.SendRaConsoleMessage(result.Response, result.StatusCodeInt == (int)CommandStatusCode.Ok);
            }
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error($"S3 Commands: RemoteAdmin command failed:\n{ex}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerConsole", PatchType.Command)]
public static class PlayerConsolePatch
{
    private static readonly SynapseCommandService _commandService;
    static PlayerConsolePatch() => _commandService = Synapse.Get<SynapseCommandService>();
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    public static bool OnPlayerConsoleCommand(QueryProcessor __instance, string query)
    {
        try
        {
            var player = __instance._sender.GetSynapsePlayer();
            if (player == null) return true;
            if (player.Hub.Mode != ClientInstanceMode.ReadyClient && player.PlayerType == PlayerType.Player)
                return true;

            var result = _commandService.PlayerConsole
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
            SynapseLogger<Synapse>.Error($"S3 Commands: PlayerConsole command failed:\n{ex}");
            return true;
        }
    }
}
#endif
