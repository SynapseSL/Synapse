using Neuron.Core.Events;
using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Events;

public class RoundEvents : Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<RoundStartEvent> Start = new();
    public readonly EventReactor<RoundEndEvent> End = new();
    public readonly EventReactor<RoundWaitingEvent> Waiting = new();
    public readonly EventReactor<RoundRestartEvent> Restart = new();
    public readonly EventReactor<RoundCheckEndEvent> CheckEnd = new();
    public readonly EventReactor<SelectTeamEvent> SelectTeam = new();

    public RoundEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }
    
    public override void Enable()
    {
        _eventManager.RegisterEvent(Start);
        _eventManager.RegisterEvent(End);
        _eventManager.RegisterEvent(Waiting);
        _eventManager.RegisterEvent(Restart);
        _eventManager.RegisterEvent(CheckEnd);
        _eventManager.RegisterEvent(SelectTeam);
    }
}

public class RoundStartEvent : IEvent { }

public class RoundEndEvent : IEvent { }

public class RoundWaitingEvent : IEvent
{
    public bool FirstTime { get; set; }
}

public class RoundRestartEvent : IEvent { }

public class RoundCheckEndEvent : IEvent
{
    public bool EndRound { get; set; }
    
    public RoundSummary.LeadingTeam WinningTeam { get; set; }
}

public class SelectTeamEvent : IEvent
{
    public int ID { get; set; }

    public bool Reset { get; set; } = false;
}