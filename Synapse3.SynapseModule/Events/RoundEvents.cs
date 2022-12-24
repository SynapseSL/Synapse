using System;
using System.Collections.Generic;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using PlayerRoles;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Events;

public partial class RoundEvents : Service
{
    private readonly EventManager _eventManager;
    private readonly Synapse _synapse;

    public readonly EventReactor<RoundStartEvent> Start = new();
    public readonly EventReactor<RoundEndEvent> End = new();
    public readonly EventReactor<RoundWaitingEvent> Waiting = new();
    public readonly EventReactor<RoundRestartEvent> Restart = new();
    public readonly EventReactor<RoundCheckEndEvent> CheckEnd = new();
    public readonly EventReactor<SelectTeamEvent> SelectTeam = new();
    public readonly EventReactor<SpawnTeamEvent> SpawnTeam = new();
    public readonly EventReactor<FirstSpawnEvent> FirstSpawn = new();
    public readonly EventReactor<DecontaminationEvent> Decontamination = new();

    public RoundEvents(EventManager eventManager, Synapse synapse)
    {
        _eventManager = eventManager;
        _synapse = synapse;
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
        PluginAPI.Events.EventManager.RegisterEvents(_synapse,this);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Start);
        _eventManager.UnregisterEvent(End);
        _eventManager.UnregisterEvent(Waiting);
        _eventManager.UnregisterEvent(Restart);
        _eventManager.UnregisterEvent(CheckEnd);
        _eventManager.UnregisterEvent(SelectTeam);
        _eventManager.UnregisterEvent(SpawnTeam);
        _eventManager.UnregisterEvent(FirstSpawn);
        _eventManager.UnregisterEvent(Decontamination);
    }
}

public class RoundStartEvent : IEvent { }

public class RoundEndEvent : IEvent
{
    public RoundEndEvent(RoundSummary.LeadingTeam winningTeam)
    {
        WinningTeam = winningTeam;
    }

    public RoundSummary.LeadingTeam WinningTeam { get; }
}

public class RoundWaitingEvent : IEvent
{
    public RoundWaitingEvent(bool firstTime)
    {
        FirstTime = firstTime;
    }

    public bool FirstTime { get; }
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
    public int AmountOfScpSpawns { get; set; }
    public Team[] HumanQueue { get; set; }
    public bool EnableLateJoin { get; set; } = true;
    public bool EnableNormalSpawning { get; set; } = true;
    public bool CustomSpawning { get; set; } = false;
}

public class DecontaminationEvent : IEvent
{
    public bool Allow { get; set; } = true;
}