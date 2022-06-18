using System;
using System.Collections.Generic;
using System.Reflection;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Event;
using Synapse3.SynapseModule.Command.SynapseCommands;

namespace Synapse3.SynapseModule.Command;

public class SynapseCommandService : Service
{
    private readonly List<Type> _synapseCommands = new List<Type>
    {
        typeof(TestCommand)
    };

    private readonly CommandService _command;

    public CommandReactor ServerConsole { get; private set; }
    public CommandReactor RemoteAdmin { get; private set; }
    public CommandReactor PlayerConsole { get; private set; }
    
    public SynapseCommandService(CommandService command)
    {
        _command = command;
    }

    public override void Enable()
    {
        ServerConsole = _command.CreateCommandReactor();
        ServerConsole.NotFoundFallbackHandler = NotFound;
        
        RemoteAdmin = _command.CreateCommandReactor();
        RemoteAdmin.NotFoundFallbackHandler = NotFound;
        
        PlayerConsole = _command.CreateCommandReactor();
        PlayerConsole.NotFoundFallbackHandler = NotFound;

        foreach (var command in _synapseCommands)
        {
            RegisterSynapseCommand(command);
        }
    }

    public void RegisterSynapseCommand(Type command)
    {
        var rawMeta = command.GetCustomAttribute(typeof(SynapseCommandAttribute));
        if(rawMeta is null) return;
        var meta = (SynapseCommandAttribute)rawMeta;

        foreach (var platform in meta.Platforms)
        {
            switch (platform)
            {
                case CommandPlatform.PlayerConsole:
                    PlayerConsole.RegisterCommand(command);
                    break;
                    
                case CommandPlatform.RemoteAdmin:
                    RemoteAdmin.RegisterCommand(command);
                    break;
                    
                case CommandPlatform.ServerConsole:
                    ServerConsole.RegisterCommand(command);
                    break;
            }
        }
    }

    private static CommandResult NotFound(CommandEvent args)
    {
        return new CommandResult()
        {
            StatusCode = 0,
            Response = "You shouldn't be able to see this since the default Game response should come"
        };
    }
}