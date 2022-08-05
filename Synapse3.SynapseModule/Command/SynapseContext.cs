using System;
using System.Linq;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Command;

/// <summary>
/// The Context in which all Synapse Commands are executed
/// </summary>
public class SynapseContext : ICommandContext
{
    /// <summary>
    /// The Name of the executed Command
    /// </summary>
    public string Command { get; set; }
    
    /// <summary>
    /// All the Arguments of the Command
    /// </summary>
    public string[] Arguments { get; set; }
    
    /// <summary>
    /// The Full string that was used to call this command
    /// </summary>
    public string FullCommand { get; set; }

    /// <summary>
    /// Returns true if the Player is the Server
    /// </summary>
    public bool IsAdmin => Platform == CommandPlatform.ServerConsole || Player.PlayerType == PlayerType.Server;

    /// <summary>
    /// Return the SynapseContext type
    /// </summary>
    public Type ContextType => typeof(SynapseContext);
    
    /// <summary>
    /// The Player which called the command
    /// </summary>
    public SynapsePlayer Player { get; private set; }
    
    /// <summary>
    /// The Command Platform in which the command was executed
    /// </summary>
    public CommandPlatform Platform { get; private set; }

    /// <summary>
    /// Creates a new SynapseContext
    /// </summary>
    public static SynapseContext Of(string message, SynapsePlayer player, CommandPlatform platform)
    {
        var context = new SynapseContext
        {
            FullCommand = message
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