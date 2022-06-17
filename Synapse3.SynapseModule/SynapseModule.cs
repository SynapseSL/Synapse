using System;
using HarmonyLib;
using Neuron.Core.Modules;
using Neuron.Modules.Patcher;
using Neuron.Modules.Commands;
using Ninject;
using Synapse3.SynapseModule.Patches;

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

    public override void Load(IKernel kernel)
    {
        Logger.Info("Synapse3 is loading");
    }

    public override void Enable()
    {
        Logger.Info("Synapse3 enabled!");
        var result = Patcher.GetPatcherInstance()
            .CreateProcessor(typeof(RoundSummary).GetMethod(nameof(RoundSummary.Start)))
            .AddPrefix(typeof(DecoratedRoundMethods).GetMethod(nameof(DecoratedRoundMethods.ProcessServerSideCode)))
            .Patch();
    }

    public override void Disable()
    {
        
    }
}