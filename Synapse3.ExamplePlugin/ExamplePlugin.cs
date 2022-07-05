using Neuron.Core.Dev;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Neuron.Core.Plugins;
using Ninject;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Events;

namespace Synapse3.ExamplePlugin
{
    [Plugin(
        Name = "Example Plugin",
        Description = "Example Description",
        Version = "1.0.0"
    )]
    public class ExamplePlugin : Plugin
    {
        [Inject]
        public ExampleConfig Config { get; set; }
        
        [Inject]
        public ExampleTranslations Translations { get; set; }

        public override void Load()
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
}