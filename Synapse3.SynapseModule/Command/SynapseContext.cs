using System;
using System.Linq;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Command;

public class SynapseContext : ICommandContext
{
    public string Command { get; set; }
    public string[] Arguments { get; set; }
    public string FullCommand { get; set; }

    public bool IsAdmin
    {
        get => Platform == CommandPlatform.ServerConsole || Player.PlayerType == PlayerType.Server;
        set {}
    }
    
    public Type ContextType => typeof(SynapseContext);
    
    public SynapsePlayer Player { get; set; }
    
    public CommandPlatform Platform { get; set; }

    public static SynapseContext Of(string message, SynapsePlayer player, CommandPlatform platform)
    {
        var context = new SynapseContext()
        {
            FullCommand = message,
            IsAdmin = true
        };
        var args = message.Split(' ').ToList();
        context.Command = args[0];
        args.RemoveAt(0);
        context.Arguments = args.ToArray();
        
        context.Player = player;
        context.Platform = platform;
        
        return context;
    }
}