using CommandSystem.Commands.Shared;
using Neuron.Core.Modules;
using Neuron.Modules.Patcher;
using Neuron.Modules.Commands;
using Neuron.Modules.Configs;
using Ninject;

namespace Synapse3.SynapseModule;

[Module(
    Name = "Synapse",
    Description = "SCP:SL game functionality",
    Dependencies = new []
    {
        typeof(PatcherModule),
        typeof(CommandsModule),
        typeof(ConfigsModule)
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
        
        CustomNetworkManager.Modded = true;
        BuildInfoCommand.ModDescription = $"Plugin Framework: Synapse\n" +
                                          $"Synapse Version: {Synapse.GetVersion()}\n" +
                                          $"Description: Synapse is a heavily modded server software using extensive runtime patching to make development faster and the usage more accessible to end-users";
        
        if(Synapse.BasedGameVersion != GameCore.Version.VersionString)
            Logger.Warn($"Sy3 Version: This Version of Synapse3 is build for SCPSL Version {Synapse.BasedGameVersion} Currently installed: {GameCore.Version.VersionString}\nBugs may occurs");
    }

    public override void Enable()
    {
        Logger.Info("Synapse3 enabled!");
    }

    public override void Disable()
    {
        
    }
}