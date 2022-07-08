using Neuron.Core.Events;

namespace Synapse3.SynapseModule.Events;

public abstract class SynapseEvent<TEvent> : IEvent where TEvent : SynapseEvent<TEvent>
{
    private EventReactor<TEvent> _reactor;

    protected SynapseEvent(EventReactor<TEvent> reactor)
    {
        _reactor = reactor;
    }

    public void Raise() => _reactor.Raise((TEvent)this);
}