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
    public readonly EventReactor<DisarmEvent> Disarm = new();

    public ItemEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(BasicInteract);
        _eventManager.RegisterEvent(KeyCardInteract);
        _eventManager.RegisterEvent(ConsumeItem);
        _eventManager.RegisterEvent(Disarm);

        KeyCardInteract.Subscribe(ev => BasicInteract.Raise(ev));
        ConsumeItem.Subscribe(ev => BasicInteract.Raise(ev));
        Disarm.Subscribe(ev => BasicInteract.Raise(ev));
    }
}

public abstract class BasicItemInteractEvent : PlayerInteractEvent
{
    public BasicItemInteractEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player) :
        base(player, true)
    {
        Item = item;
        State = state;
    }
    
    public SynapseItem Item { get; }
    
    public ItemInteractState State { get; }
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

public class DisarmEvent : BasicItemInteractEvent
{
    public DisarmEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player, SynapsePlayer target) : base(item, state, player)
    {
        Target = target;
    }

    public SynapsePlayer Target { get; }
}

public class FlipCoinEvent : BasicItemInteractEvent
{
    public FlipCoinEvent(SynapseItem item, SynapsePlayer player, bool tails) : base(item, ItemInteractState.Finalize, player)
    {
        Tails = tails;
    }

    public bool Tails { get; set; }
}