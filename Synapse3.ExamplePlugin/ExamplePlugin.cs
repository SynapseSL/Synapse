using Neuron.Core.Dev;
using Neuron.Core.Plugins;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;

namespace Synapse3.ExamplePlugin;

[Plugin(
    Name = "Example Plugin",
    Description = "Example Description",
    Version = "1.0.0",
    Author = "Helight & Dimenzio"
)]
public class ExamplePlugin : ReloadablePlugin<ExampleConfig, ExampleTranslations>
{
    public ExampleEventHandler EventHandler { get; set; }
    public ElevatorEventHandler ElevatorHandler { get; set; }
    public RoomEventHandler RoomHandler { get; set; }

    public override void EnablePlugin()
    {
        //Translations.Get will ensure that the Server Translation will be used if one exists
        Logger.Info(Translation.Get().EnableMessage);
            
        //Get and Bind ensures that you can always get the same instance everywhere, although it is not used and Synapse.Get would be enough for this case
        EventHandler = Synapse.GetAndBind<ExampleEventHandler>();
        ElevatorHandler = Synapse.GetAndBind<ElevatorEventHandler>();
        RoomHandler = Synapse.GetAndBind<RoomEventHandler>();
    }
}

public class ExampleEventHandler
{
    private readonly ExamplePlugin _pluginClass;

    public ExampleEventHandler(ItemEvents itemEvents, ExamplePlugin pluginClass)
    {
        _pluginClass = pluginClass;
            
        itemEvents.ConsumeItem.Subscribe(Consume);
    }

    private void Consume(ConsumeItemEvent ev)
    {
        if (ev.State == ItemInteractState.Finalize)
            ev.Player.SendBroadcast(
                _pluginClass.Translation.Get(ev.Player).ConsumeItemMessage.Format(ev.Item.Name), 5);
    }
}
