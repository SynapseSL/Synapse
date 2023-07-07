using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Event;
using RemoteAdmin;
using Synapse3.SynapseModule.Command.SynapseCommands;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Command;

public class SynapseCommandService : Service
{
    /// <summary>
    /// A List of all Default Commands that will be added manually
    /// </summary>
    private readonly List<Type> _synapseCommands = new()
    {
        typeof(PermissionCommand),
        typeof(ReloadCommand),
        typeof(KeyPressCommand),
        typeof(SetClassCommand),
        typeof(RespawnCommand),
        typeof(GiveCustomItemCommand),
        typeof(HelpCommand),
        typeof(RoomPointCommand),
        typeof(SchematicCommand),
        typeof(PluginCommand),
        typeof(LanguageCommand),
        typeof(KeyBindCommand),
        typeof(ScpProximityCommand),
        typeof(RolesCommand)
    };
    
    private readonly RoundEvents _round;
    private readonly Synapse _synapseModule;

    /// <summary>
    /// The <see cref="CommandReactor"/> for all Server Console Commands
    /// </summary>
    public CommandReactor ServerConsole { get; private set; }
    /// <summary>
    /// The <see cref="CommandReactor"/> for all Remote Admin Commands
    /// </summary>
    public CommandReactor RemoteAdmin { get; private set; }
    /// <summary>
    /// The <see cref="CommandReactor"/> for all Player Console Commands
    /// </summary>
    public CommandReactor PlayerConsole { get; private set; }

    /// <summary>
    /// Creates a new Instance of the SynapseCommandService
    /// </summary>
    public SynapseCommandService(CommandService command,RoundEvents round, Synapse synapseModule)
    {
        _round = round;
        _synapseModule = synapseModule;
        
        ServerConsole = command.CreateCommandReactor();
        ServerConsole.NotFoundFallbackHandler = NotFound;
        
        RemoteAdmin = command.CreateCommandReactor();
        RemoteAdmin.NotFoundFallbackHandler = NotFound;
        
        PlayerConsole = command.CreateCommandReactor();
        PlayerConsole.NotFoundFallbackHandler = NotFound;
    }

    /// <summary>
    /// Set's up everything for the SynapseCommandService. Don't call it manually
    /// </summary>
    public override void Enable()
    {
        while (_synapseModule.ModuleCommandBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleCommandBindingQueue.Dequeue();
            LoadBinding(binding);
        }
        
        foreach (var command in _synapseCommands)
        {
            RegisterSynapseCommand(command);
        }
        
        _round.Waiting.Subscribe(GenerateCommandCompletion);
    }

    /// <summary>
    /// Disables the SynapseCommandService. Don't call it manually
    /// </summary>
    public override void Disable()
    {
        _round.Waiting.Unsubscribe(GenerateCommandCompletion);
    }

    internal void LoadBinding(SynapseCommandBinding binding) => RegisterSynapseCommand(binding.Type);

    /// <summary>
    /// Registers a new SynapseCommand with just it type
    /// </summary>
    public void RegisterSynapseCommand(Type command)
    {
        var rawMeta = command.GetCustomAttribute(typeof(SynapseCommandAttribute));
        if (rawMeta == null) return;
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

    /// <summary>
    /// Returns the default Command NotFound Message
    /// </summary>
    private static CommandResult NotFound(CommandEvent args)
    {
        return new CommandResult
        {
            StatusCode = 0,
            Response = "You shouldn't be able to see this since the default game response should come"
        };
    }

    /// <summary>
    /// This is for generating the Command Completion a player can see when entering a Command in the Remote Admin
    /// </summary>
    private void GenerateCommandCompletion(RoundWaitingEvent _)
    {
        var list = QueryProcessor.ParseCommandsToStruct(CommandProcessor.GetAllCommands()).ToList();
        list.Remove(list.FirstOrDefault(x => x.Command == "give"));

        foreach (var command in RemoteAdmin.Handler.Commands)
        {
            if(command.Meta is not SynapseRaCommandAttribute meta) continue;

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