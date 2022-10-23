using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Command;

namespace Synapse3.ExampleModule;

[Automatic]
[SynapseCommand(
    CommandName = "Example2",
    Description = "An example command",
    Aliases = new[] {"Ex2"},
    Platforms = new[] {CommandPlatform.RemoteAdmin, CommandPlatform.PlayerConsole, CommandPlatform.ServerConsole}
)]
public class ExampleCommand : SynapseCommand
{
    private readonly ExampleModule _module;
    
    public ExampleCommand(ExampleModule module)
    {
        _module = module;
    }
    
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        result.Response = _module.Translation.Get(context.Player).CommandMessage + _module.Config.ConfigValue;
    }
}
    
        
