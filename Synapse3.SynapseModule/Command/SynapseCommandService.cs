using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Event;
using RemoteAdmin;
using Synapse3.SynapseModule.Command.SynapseCommands;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Command;

public class SynapseCommandService : Service
{
    private readonly List<Type> _synapseCommands = new List<Type>
    {
        typeof(TestCommand)
    };
    
    private readonly CommandService _command;
    private readonly RoundEvents _round;
    private readonly SynapseModule _synapseModule;

    public CommandReactor ServerConsole { get; private set; }
    public CommandReactor RemoteAdmin { get; private set; }
    public CommandReactor PlayerConsole { get; private set; }

    public SynapseCommandService(CommandService command,RoundEvents round, SynapseModule synapseModule)
    {
        _command = command;
        _round = round;
        _synapseModule = synapseModule;
    }

    public override void Enable()
    {
        ServerConsole = _command.CreateCommandReactor();
        ServerConsole.NotFoundFallbackHandler = NotFound;
        
        RemoteAdmin = _command.CreateCommandReactor();
        RemoteAdmin.NotFoundFallbackHandler = NotFound;
        
        PlayerConsole = _command.CreateCommandReactor();
        PlayerConsole.NotFoundFallbackHandler = NotFound;

        while (_synapseModule.moduleCommandBindingQueue.Count != 0)
        {
            var binding = _synapseModule.moduleCommandBindingQueue.Dequeue();
            LoadBinding(binding);
        }
        
        foreach (var command in _synapseCommands)
        {
            RegisterSynapseCommand(command);
        }
        
        _round.RoundWaiting.Subscribe(GenerateCommandCompletion);
    }
    
    public void LoadBinding(SynapseCommandBinding binding) => RegisterSynapseCommand(binding.Type);

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
            Response = "You shouldn't be able to see this since the default game response should come"
        };
    }

    private void GenerateCommandCompletion(RoundWaitingEvent ev)
    {
        var list = QueryProcessor.ParseCommandsToStruct(CommandProcessor.GetAllCommands()).ToList();

        foreach (var command in RemoteAdmin.Handler.Commands)
        {
            var meta = (SynapseRaCommandAttribute)command.Meta;
            list.Add(new QueryProcessor.CommandData
            {
                Command = meta.CommandName,
                AliasOf = null,
                Description = meta.Description,
                Hidden = false,
                Usage = meta.Parameters
            });
            
            if(meta.Aliases == null) continue;

            foreach (var alias in meta.Aliases)
            {
                list.Add(new QueryProcessor.CommandData
                {
                    Command = alias,
                    AliasOf = meta.CommandName,
                    Description = meta.Description,
                    Hidden = false,
                    Usage = meta.Parameters
                });
            }
        }

        QueryProcessor._commands = list.ToArray();
    }
}