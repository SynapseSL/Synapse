using Neuron.Core.Dev;
using Neuron.Core.Plugins;
using Ninject;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;

namespace Synapse3.ExamplePlugin
{
    [Plugin(
        Name = "Example Plugin",
        Description = "Example Description",
        Version = "1.0.0",
        Author = "Helight & Dimenzio"
    )]
    public class ExamplePlugin : Plugin
    {
        //It's recommended to store your Config/Translation Reference inside your ExamplePlugin so that you can just reference your ExamplePlugin and update the instances properly
        public ExampleConfig Config { get; set; }
        public ExampleTranslations Translations { get; set; }
        
        
        [Inject]
        public ServerEvents ServerEvents { get; set; }
        
        
        public ExampleEventHandler EventHandler { get; set; }
        public ElevatorEventHandler ElevatorHandler { get; set; }
        public RoomEventHandler RoomHandler { get; set; }

        public override void Load()
        {
            //Get and Bind ensures that you can always get the same instance everywhere, although it is not used and Synapse.Get would be enough for this case
            EventHandler = Synapse.GetAndBind<ExampleEventHandler>();
            ElevatorHandler = Synapse.GetAndBind<ElevatorEventHandler>();
            RoomHandler = Synapse.GetAndBind<RoomEventHandler>();
        }

        public override void Enable()
        {
            //Reload will set Config and Translation (they are both created and binded when you use the Automatic Attribute)
            Reload();
            
            //Translations.Get will ensure that the Server Translation will be used if one exists
            Logger.Info(Translations.Get().EnableMessage);
            
            ElevatorHandler.HookEvents();
            RoomHandler.HookEvents();
            EventHandler.HookEvents();
            ServerEvents.Reload.Subscribe(Reload);
        }

        private void Reload(ReloadEvent _ = null)
        {
            //All Config References need to be updated after a Reload
            Config = Synapse.Get<ExampleConfig>();
            
            //Translation References only need to be Updated if you want to use the Default Translation and not the proper Player/Server Translation (you don't use Translation.Get)
            Translations = Synapse.Get<ExampleTranslations>();
        }
    }

    public class ExampleEventHandler
    {
        private readonly ItemEvents _itemEvents;
        private readonly ExamplePlugin _pluginClass;

        public ExampleEventHandler(ItemEvents itemEvents, ExamplePlugin pluginClass)
        {
            _itemEvents = itemEvents;
            _pluginClass = pluginClass;
        }

        public void HookEvents()
        {
            _itemEvents.ConsumeItem.Subscribe(Consume);
        }

        private void Consume(ConsumeItemEvent ev)
        {
            if (ev.State == ItemInteractState.Finalize)
                ev.Player.SendBroadcast(
                    _pluginClass.Translations.Get(ev.Player).ConsumeItemMessage.Format(ev.Item.Name), 5);
        }
    }
}