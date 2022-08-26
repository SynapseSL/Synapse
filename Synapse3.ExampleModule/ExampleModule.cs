using Neuron.Core.Modules;
using Synapse3.SynapseModule;

namespace Synapse3.ExampleModule;

[Module(
    Name = "Example Module",
    Description = "Example Description",
    Dependencies = new[]
    {
        typeof(Synapse)
    }
)]
public class ExampleModule : ReloadableModule<ExampleConfig, ExampleTranslations>
{
    public override void EnableModule()
    {
        Logger.Info(Translation.Get().EnableMessage);
    }
}