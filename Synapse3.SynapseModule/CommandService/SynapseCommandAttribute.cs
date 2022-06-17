using Neuron.Modules.Commands.Command;

namespace Synapse3.SynapseModule.CommandService;

public class SynapseCommandAttribute : CommandAttribute
{
    public string Permission { get; set; } = "";

    public CommandPlatform[] Platforms { get; set; } = { CommandPlatform.ServerConsole };
}

public class SynapseRACommandAttribute : SynapseCommandAttribute
{
    public string[] Parameters { get; set; } = { };
}