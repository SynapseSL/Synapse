using Neuron.Core.Dev;
using Neuron.Core.Events;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
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
    public ElevatorEventHandler ElevatorHandler { get; set; }
    public RoomEventHandler RoomHandler { get; set; }

    public override void EnablePlugin()
    {
        //Translations.Get will ensure that the Server Translation will be used if one exists
        Logger.Info(Translation.Get().EnableMessage);
            
        //Get and Bind ensures that you can always get the same instance everywhere, although it is not used and Synapse.Get would be enough for this case
        ElevatorHandler = Synapse.GetAndBind<ElevatorEventHandler>();
        RoomHandler = Synapse.GetAndBind<RoomEventHandler>();
    }
}

[Automatic]
public class ExampleEventHandler : Listener
{
    private readonly ExamplePlugin _pluginClass;

    public ExampleEventHandler(ExamplePlugin pluginClass)
    {
        _pluginClass = pluginClass;
    }

    [EventHandler]
    public void StartRound(RoundStartEvent ev)
    {
        NeuronLogger.For<ExamplePlugin>().Warn("START ROUND");
    }

    [EventHandler]
    public void Consume(ConsumeItemEvent ev)
    {
        NeuronLogger.For<ExamplePlugin>().Warn("Consume " + ev.State);
        if (ev.State == ItemInteractState.Finalize)
        {
            NeuronLogger.For<ExamplePlugin>().Warn("Consume Finalize");
            ev.Player.SendBroadcast(
                _pluginClass.Translation.Get(ev.Player).ConsumeItemMessage.Format(ev.Item.Name), 5);
        }
    }
}
