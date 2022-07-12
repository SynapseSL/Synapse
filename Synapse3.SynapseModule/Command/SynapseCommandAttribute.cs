using Neuron.Modules.Commands.Command;

namespace Synapse3.SynapseModule.Command;

/// <summary>
/// The Attribute that must have all types that should be registered as Synapse Command
/// </summary>
public class SynapseCommandAttribute : CommandAttribute
{
    /// <summary>
    /// The Required Permission for this Command that the Player needs
    /// </summary>
    public string Permission { get; set; } = "";

    /// <summary>
    /// All the Supported Platforms of this Command
    /// </summary>
    public CommandPlatform[] Platforms { get; set; } = { CommandPlatform.ServerConsole };
}

/// <summary>
/// An Extension of the default <see cref="SynapseCommandAttribute"/> that also Contains Remote Admin Parameters that can be displayed in the RemoteAdmin
/// </summary>
public class SynapseRaCommandAttribute : SynapseCommandAttribute
{
    /// <summary>
    /// An Array of all Parameters that should be displayed to the player as command completion help
    /// </summary>
    public string[] Parameters { get; set; } = { };
}