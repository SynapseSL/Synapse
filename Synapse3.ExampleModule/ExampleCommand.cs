using System.Diagnostics;
using System.IO.Compression;
using Neuron.Core.Dev;
using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using Ninject;
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

    [Inject] public ExampleConfig Config { get; set; }

    [Inject] public ExampleTranslations Translations { get; set; }

    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        Logger.Fatal(Config);
    }
}
    
        
