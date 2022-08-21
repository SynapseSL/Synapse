using Neuron.Core.Meta;
using Neuron.Modules.Configs.Localization;

namespace Synapse3.ExamplePlugin;

[Automatic]
public class ExampleTranslations : Translations<ExampleTranslations>
{
    public string EnableMessage { get; set; } = "Plugin will be enabled!";

    public string ConsumeItemMessage { get; set; } = "You just used an {0}";
}