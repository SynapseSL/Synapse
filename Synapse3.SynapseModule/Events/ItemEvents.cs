using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;

namespace Synapse3.SynapseModule.Events;

public class ItemEvents: Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<BasicItemInteractEvent> BasicInteract = new();
    public readonly EventReactor<KeyCardInteractEvent> KeyCardInteract = new ();

    public ItemEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(KeyCardInteract);

        KeyCardInteract.Subscribe(ev => BasicInteract.Raise(ev));
    }
}

public abstract class BasicItemInteractEvent : IEvent
{
    public BasicItemInteractEvent(SynapseItem item, ItemInteractState state)
    {
        Item = item;
        State = state;
    }
    
    public SynapseItem Item { get; }
    
    public ItemInteractState State { get; }
    
    public bool Allow { get; set; }
}

public class KeyCardInteractEvent : BasicItemInteractEvent
{
    public KeyCardInteractEvent(SynapseItem item, ItemInteractState state) : base(item, state) { }
}