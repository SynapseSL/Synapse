using System;
using System.Linq;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.CommandService;

public class SynapseContext : ICommandContext
{
    public string Command { get; set; }
    public string[] Arguments { get; set; }
    public string FullCommand { get; set; }

    public bool IsAdmin
    {
        get => false;
        set {}
    }
    
    public Type ContextType => typeof(SynapseContext);
    
    public SynapsePlayer Player { get; set; }

    public SynapseContext Of(string message, SynapsePlayer player)
    {
        Player = player;
        
        var context = new SynapseContext()
        {
            FullCommand = message,
            IsAdmin = true
        };
        var args = message.Split(' ').ToList();
        context.Command = args[0];
        args.RemoveAt(0);
        context.Arguments = args.ToArray();
        return context;
    }
}