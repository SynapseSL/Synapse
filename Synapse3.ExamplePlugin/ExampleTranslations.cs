using Neuron.Core.Meta;
using Neuron.Modules.Configs.Localization;

namespace Synapse3.ExamplePlugin;

[Automatic]
public class ExampleTranslations : Translations<ExampleTranslations>
{
    public string CommandMessage { get; set; } = "You just executed the {0} example command, {1}!";
}