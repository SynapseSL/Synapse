using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public class MapEvents : Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<Scp914UpgradeEvent> Scp914Upgrade = new();
    public readonly EventReactor<DoorInteractEvent> DoorInteract = new();

    public MapEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Scp914Upgrade);
        _eventManager.RegisterEvent(DoorInteract);
    }
}

public class Scp914UpgradeEvent : IEvent
{
    public ReadOnlyCollection<SynapsePlayer> Players { get; }
    
    public ReadOnlyCollection<SynapseItem> Items { get; }

    public bool Allow { get; set; } = true;

    public bool MovePlayers { get; set; } = true;

    public bool MoveItems { get; set; } = true;
    
    public Vector3 MoveVector { get; set; }

    public Scp914UpgradeEvent(List<SynapsePlayer> players, List<SynapseItem> items)
    {
        Players = new ReadOnlyCollection<SynapsePlayer>(players);
        Items = new ReadOnlyCollection<SynapseItem>(items);
    }
}

public class DoorInteractEvent : PlayerInteractEvent
{
    public SynapseDoor Door { get; private set; }
    
    /// <summary>
    /// This is true when a player tries to open/close a locked door and he is not in Bypass or something else causes to overrides the lock like the Nuke
    /// </summary>
    public bool LockBypassRejected { get; set; }

    public DoorInteractEvent(SynapsePlayer player, bool allow, SynapseDoor door, bool lockBypass) : base(player, allow)
    {
        Door = door;
        LockBypassRejected = lockBypass;
    }
}