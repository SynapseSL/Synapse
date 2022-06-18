using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Command;

namespace Synapse3.ExamplePlugin;

[Automatic]
[SynapseCommand(
    CommandName = "Example",
    Description = "An example command",
    Aliases = new []{"Ex"},
    Platforms = new [] {CommandPlatform.RemoteAdmin, CommandPlatform.PlayerConsole, CommandPlatform.ServerConsole}
)]
public class ExampleCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        result.Response = "Yep worked somehow";
    }
}