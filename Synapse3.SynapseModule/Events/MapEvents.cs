using System.Collections.Generic;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public partial class MapEvents : Service
{
    private readonly EventManager _eventManager;
    private readonly Synapse _synapse;

    public readonly EventReactor<Scp914UpgradeEvent> Scp914Upgrade = new();
    public readonly EventReactor<ElevatorMoveContentEvent> ElevatorMoveContent = new();
    public readonly EventReactor<TriggerTeslaEvent> TriggerTesla = new();
    public readonly EventReactor<DetonateWarheadEvent> DetonateWarhead = new();
    public readonly EventReactor<CancelWarheadEvent> CancelWarhead = new();
    public readonly EventReactor<GeneratorEngageEvent> GeneratorEngage = new();

    public MapEvents(EventManager eventManager, Synapse synapse)
    {
        _eventManager = eventManager;
        _synapse = synapse;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Scp914Upgrade);
        _eventManager.RegisterEvent(ElevatorMoveContent);
        _eventManager.RegisterEvent(TriggerTesla);
        _eventManager.RegisterEvent(DetonateWarhead);
        _eventManager.RegisterEvent(CancelWarhead);
        _eventManager.RegisterEvent(GeneratorEngage);
        
        PluginAPI.Events.EventManager.RegisterEvents(_synapse,this);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Scp914Upgrade);
        _eventManager.UnregisterEvent(ElevatorMoveContent);
        _eventManager.UnregisterEvent(TriggerTesla);
        _eventManager.UnregisterEvent(DetonateWarhead);
        _eventManager.UnregisterEvent(CancelWarhead);
        _eventManager.UnregisterEvent(GeneratorEngage);
    }
}

public class Scp914UpgradeEvent : IEvent
{
    public List<SynapsePlayer> Players { get; }
    
    public List<SynapseItem> Items { get; }

    public bool Allow { get; set; } = true;

    public bool MovePlayers { get; set; } = true;

    public bool MoveItems { get; set; } = true;
    
    public Vector3 MoveVector { get; set; }

    public Scp914UpgradeEvent(List<SynapsePlayer> players, List<SynapseItem> items)
    {
        Players = players;
        Items = items;
    }
}

public class GeneratorEngageEvent : IEvent
{
    public SynapseGenerator Generator { get; }

    public bool Allow { get; set; } = true;
    
    internal bool ForcedUnAllow;

    public GeneratorEngageEvent(SynapseGenerator gen)
    {
        Generator = gen;
    }

    public void ResetTime()
    {
        ForcedUnAllow = true;
        Generator.Generator._currentTime = 0;
        Generator.Generator.Network_syncTime = 0;
    }

    public void Deactivate(bool resetTime = true)
    {
        ForcedUnAllow = true;
        Generator.Generator.Activating = false;
        if (resetTime)
            ResetTime();
    }
}

public class ElevatorMoveContentEvent : IEvent
{
    public IElevator Elevator { get; }

    public float OpenManuallyDelay { get; set; } = 4f;

    public bool OpenDoorManually { get; set; } = false;
    
    public IElevatorDestination Destination { get; set; }

    public ElevatorMoveContentEvent(IElevator elevator)
    {
        Elevator = elevator;
    }
}

public class TriggerTeslaEvent : PlayerInteractEvent
{
    public TriggerTeslaEvent(SynapsePlayer player, bool allow, SynapseTesla tesla) : base(player, allow)
    {
        Tesla = tesla;
    }
    
    public SynapseTesla Tesla { get; }
}

public class DetonateWarheadEvent : IEvent { }

public class CancelWarheadEvent : PlayerInteractEvent
{
    public CancelWarheadEvent(SynapsePlayer player, bool allow) : base(player, allow) { }
}