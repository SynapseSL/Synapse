using Neuron.Core.Events;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;

namespace Synapse3.ExampleModule;

[Automatic]
public class ExampleEventHandler : Listener
{
    private readonly MapService _map;
    
    public ExampleEventHandler(MapService map)
    {
        _map = map;
    }

    [EventHandler]
    public void Waiting(RoundWaitingEvent ev)
    {
        NeuronLogger.For<ExampleModule>().Warn("Waiting Event Test Count: " + _map.SynapseObjects.Count);
    }
}