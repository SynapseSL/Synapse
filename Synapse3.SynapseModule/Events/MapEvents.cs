using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Events;

public class MapEvents : Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<Scp914UpgradeEvent> Scp914Upgrade = new();

    public MapEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Scp914Upgrade);
    }
}

public class Scp914UpgradeEvent : IEvent
{
    public ReadOnlyCollection<SynapsePlayer> Players { get; }
    
    public ReadOnlyCollection<SynapseItem> Items { get; }

    public bool Allow { get; set; } = true;

    public bool MovePlayers { get; set; } = true;

    public bool MoveItems { get; set; } = true;

    public Scp914UpgradeEvent(List<SynapsePlayer> players, List<SynapseItem> items)
    {
        Players = new ReadOnlyCollection<SynapsePlayer>(players);
        Items = new ReadOnlyCollection<SynapseItem>(items);
    }
}