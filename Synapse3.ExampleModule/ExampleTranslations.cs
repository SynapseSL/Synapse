using Neuron.Core.Meta;
using Neuron.Modules.Configs.Localization;

namespace Synapse3.ExampleModule;

[Automatic]
public class ExampleTranslations : Translations<ExampleTranslations>
{
    public string EnableMessage { get; set; } = "Example Module was just Enabled";

    public string CommandMessage { get; set; } = "Config Value is: ";
}