using Neuron.Core.Events;
using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Events;

public class ServerEvents: Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<ReloadEvent> Reload = new();

    public ServerEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Reload);
    }
}

public class ReloadEvent : IEvent { }