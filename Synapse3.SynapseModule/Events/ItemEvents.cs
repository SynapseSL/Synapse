using System.Numerics;
using InventorySystem.Items.Radio;
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
    public readonly EventReactor<ShootEvent> Shoot = new();
    public readonly EventReactor<ReloadWeaponEvent> ReloadWeapon = new();
    public readonly EventReactor<DisarmEvent> Disarm = new();
    public readonly EventReactor<FlipCoinEvent> FlipCoin = new();
    public readonly EventReactor<RadioUseEvent> RadioUse = new();

    public ItemEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(BasicInteract);
        _eventManager.RegisterEvent(KeyCardInteract);
        _eventManager.RegisterEvent(ConsumeItem);
        _eventManager.RegisterEvent(Shoot);
        _eventManager.RegisterEvent(ReloadWeapon);
        _eventManager.RegisterEvent(Disarm);
        _eventManager.RegisterEvent(FlipCoin);
        _eventManager.RegisterEvent(RadioUse);

        KeyCardInteract.Subscribe(ev => BasicInteract.Raise(ev));
        ConsumeItem.Subscribe(ev => BasicInteract.Raise(ev));
        Shoot.Subscribe(ev => BasicInteract.Raise(ev));
        ReloadWeapon.Subscribe(ev => BasicInteract.Raise(ev));
        Disarm.Subscribe(ev => BasicInteract.Raise(ev));
        FlipCoin.Subscribe(ev => BasicInteract.Raise(ev));
        RadioUse.Subscribe(ev => BasicInteract.Raise(ev));
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

public class RadioUseEvent : BasicItemInteractEvent
{
    public RadioUseEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player, RadioMessages.RadioCommand radioCommand, RadioMessages.RadioRangeLevel currentRange, RadioMessages.RadioRangeLevel nextRange) : base(item, state, player)
    {
        RadioCommand = radioCommand;
        CurrentRange = currentRange;
        NextRange = nextRange;
    }

    public RadioMessages.RadioCommand RadioCommand { get; }
    
    public RadioMessages.RadioRangeLevel CurrentRange { get; }
    
    public RadioMessages.RadioRangeLevel NextRange { get; set; }
}

public class ReloadWeaponEvent : BasicItemInteractEvent
{
    public ReloadWeaponEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player, bool playAnimationOverride) : base(item, state, player)
    {
        PlayAnimationOverride = playAnimationOverride;
    }

    public bool PlayAnimationOverride { get; set; }
}

public class ShootEvent : BasicItemInteractEvent
{
    public ShootEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player, SynapsePlayer target) : base(item, state, player)
    {
        Target = target;
    }

    public SynapsePlayer Target { get; }
}