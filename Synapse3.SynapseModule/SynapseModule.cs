using System;
using Neuron.Core.Modules;
using Neuron.Modules.Patcher;
using Neuron.Modules.Commands;
using Ninject;

namespace Synapse3.SynapseModule;

[Module(
    Name = "Synapse",
    Description = "SCP:SL game functionality",
    Dependencies = new []
    {
        typeof(PatcherModule),
        typeof(CommandsModule)
    }
)]
public class SynapseModule : Module
{
    [Inject]
    public PatcherService Patcher { get; set; }
    
    [Inject]
    public CommandService Commands { get; set; }

    public override void Load()
    {
        Logger.Info("Synapse3 is loading");
    }

    public override void Enable()
    {
        Logger.Info("Synapse3 enabled!");
        throw new Exception("Tests!");
    }

    public override void Disable()
    {
        
    }
}