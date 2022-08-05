using System.Collections.Generic;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

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
    public readonly EventReactor<SpawnTeamEvent> SpawnTeam = new();
    public readonly EventReactor<FirstSpawnEvent> FirstSpawn = new();
    public readonly EventReactor<DecontaminationEvent> Decontamination = new();

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
        _eventManager.RegisterEvent(SpawnTeam);
        _eventManager.RegisterEvent(FirstSpawn);
        _eventManager.RegisterEvent(Decontamination);
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
    public uint TeamId { get; set; }

    public bool Reset { get; set; } = false;
}

public class SpawnTeamEvent : IEvent
{
    public SpawnTeamEvent(uint teamId)
    {
        TeamId = teamId;
    }
    
    public uint TeamId { get; }
    
    public List<SynapsePlayer> Players { get; set; }

    public bool Allow { get; set; } = true;
}

public class FirstSpawnEvent : IEvent
{
    public Dictionary<SynapsePlayer,uint> PlayerAndRoles { get; set; }
}

public class DecontaminationEvent : IEvent
{
    public bool Allow { get; set; } = true;
}