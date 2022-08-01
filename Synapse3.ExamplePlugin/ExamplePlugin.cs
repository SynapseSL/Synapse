using Neuron.Core.Dev;
using Neuron.Core.Plugins;
using Ninject;

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
        
        public ElevatorEventHandler ElevatorHandler { get; set; }
        
        public RoomEventHandler RoomHandler { get; set; }

        public override void Load()
        {
            Logger.Info($"Before {Config}");
            Logger.Info($"Before {Translations}");
            ElevatorHandler = new ElevatorEventHandler();
            RoomHandler = new RoomEventHandler();
        }

        public override void Enable()
        {
            Logger.Info(Config.StringEntry);
            Logger.Info(Config.IntEntry);
            Logger.Info(Config.ListEntry);
            Logger.Info(Translations.CommandMessage.Format("Example Command", "Helight"));
            ElevatorHandler.HookEvents();
            RoomHandler.HookEvents();
        }

        public override void Disable()
        {
            ElevatorHandler.UnHookEvents();
            RoomHandler.UnHookEvents();
        }
    }
}