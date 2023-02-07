using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using Ninject;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Player;

namespace Synapse3.ExamplePlugin;

[Automatic]
[SynapseCommand(
    CommandName = "Example",
    Description = "An example command",
    Aliases = new []{ "Ex" },
    Platforms = new [] { CommandPlatform.RemoteAdmin, CommandPlatform.PlayerConsole, CommandPlatform.ServerConsole }
)]
public class ExampleCommand : SynapseCommand
{
    private ExamplePlugin _plugin;
    
    //You can use the constructor or an Injected Field
    [Inject]
    public PlayerService PlayerService { get; set; }

    public ExampleCommand(ExamplePlugin plugin)
    {
        _plugin = plugin;
    }
    
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        Logger.Warn("Injected" + (NeuronLoggerInjected != null));
        result.Response = "Config Value is: " + _plugin.Config.ConfigValue + " Service null: " + (PlayerService == null);
    }
}