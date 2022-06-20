using Neuron.Core.Events;
using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Events;

public class RoundEvents : Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<RoundStartEvent> RoundStart = new();
    public readonly EventReactor<RoundEndEvent> RoundEnd = new();
    public readonly EventReactor<RoundWaitingEvent> RoundWaiting = new();
    
    public RoundEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }
    
    public override void Enable()
    {
        _eventManager.RegisterEvent(RoundStart);
        _eventManager.RegisterEvent(RoundEnd);
        _eventManager.RegisterEvent(RoundWaiting);
    }
}

public class RoundStartEvent : IEvent { }

public class RoundEndEvent : IEvent { }

public class RoundWaitingEvent : IEvent
{
    public bool FirstTime { get; set; }
}