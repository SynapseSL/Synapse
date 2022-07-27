using InventorySystem.Items.Usables;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Events;

public class ItemEvents: Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<BasicItemInteractEvent> BasicInteract = new();
    public readonly EventReactor<KeyCardInteractEvent> KeyCardInteract = new ();
    public readonly EventReactor<ConsumeItemEvent> ConsumeItem = new();

    public ItemEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(BasicInteract);
        _eventManager.RegisterEvent(KeyCardInteract);
        _eventManager.RegisterEvent(ConsumeItem);

        KeyCardInteract.Subscribe(ev => BasicInteract.Raise(ev));
        ConsumeItem.Subscribe(ev => BasicInteract.Raise(ev));
    }
}

public abstract class BasicItemInteractEvent : IEvent
{
    public BasicItemInteractEvent(SynapseItem item, ItemInteractState state,SynapsePlayer player)
    {
        Item = item;
        State = state;
        Player = player;
    }
    
    public SynapseItem Item { get; }
    
    public ItemInteractState State { get; }
    
    public SynapsePlayer Player { get; }

    public bool Allow { get; set; } = true;
}

public class KeyCardInteractEvent : BasicItemInteractEvent
{
    public KeyCardInteractEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player) : base(item, state,player) { }
}

public class ConsumeItemEvent : BasicItemInteractEvent
{
    public ConsumeItemEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player,float remainingCoolDown, PlayerHandler handler) : base(item, state, player)
    {
        RemainingCoolDown = remainingCoolDown;
        Handler = handler;
    }

    public float RemainingCoolDown { get; }
    
    public PlayerHandler Handler { get; }

    public int CandyID { get; set; } = -1;
}