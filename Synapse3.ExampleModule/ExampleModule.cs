using Neuron.Core.Dev;
using Neuron.Core.Modules;
using Ninject;

namespace Synapse3.ExampleModule;

[Module(
    Name = "Example Module",
    Description = "Example Description",
    Dependencies = new []
    {
        typeof(SynapseModule.SynapseModule)
    }
)]
public class ExampleModule : Module
{
    [Inject]
    public ExampleConfig Config { get; set; }
        
    [Inject]
    public ExampleTranslations Translations { get; set; }

    public override void Load(IKernel kernel)
    {
        Logger.Info($"Before {Config}");
        Logger.Info($"Before {Translations}");
    }

    public override void Enable()
    {
        Logger.Info(Config.StringEntry);
        Logger.Info(Config.IntEntry);
        Logger.Info(Config.ListEntry);
        Logger.Info(Translations.CommandMessage.Format("Example Command", "Helight"));

    }

    public override void Disable()
    {
            
    }
}