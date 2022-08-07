using InventorySystem.Items.MicroHID;
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
    public readonly EventReactor<ThrowGrenadeEvent> ThrowGrenade = new();
    public readonly EventReactor<MicroUseEvent> MicroUse = new();


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
        _eventManager.RegisterEvent(ThrowGrenade);
        _eventManager.RegisterEvent(MicroUse);

        KeyCardInteract.Subscribe(BasicInteract.Raise);
        ConsumeItem.Subscribe(BasicInteract.Raise);
        Shoot.Subscribe(BasicInteract.Raise);
        ReloadWeapon.Subscribe(BasicInteract.Raise);
        Disarm.Subscribe(BasicInteract.Raise);
        FlipCoin.Subscribe(BasicInteract.Raise);
        RadioUse.Subscribe(BasicInteract.Raise);
        ThrowGrenade.Subscribe(BasicInteract.Raise);
        MicroUse.Subscribe(BasicInteract.Raise);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(BasicInteract);
        _eventManager.UnregisterEvent(KeyCardInteract);
        _eventManager.UnregisterEvent(ConsumeItem);
        _eventManager.UnregisterEvent(Shoot);
        _eventManager.UnregisterEvent(ReloadWeapon);
        _eventManager.UnregisterEvent(Disarm);
        _eventManager.UnregisterEvent(FlipCoin);
        _eventManager.UnregisterEvent(RadioUse);
        _eventManager.UnregisterEvent(ThrowGrenade);
        _eventManager.UnregisterEvent(MicroUse);
        
        KeyCardInteract.Unsubscribe(BasicInteract.Raise);
        ConsumeItem.Unsubscribe(BasicInteract.Raise);
        Shoot.Unsubscribe(BasicInteract.Raise);
        ReloadWeapon.Unsubscribe(BasicInteract.Raise);
        Disarm.Unsubscribe(BasicInteract.Raise);
        FlipCoin.Unsubscribe(BasicInteract.Raise);
        RadioUse.Unsubscribe(BasicInteract.Raise);
        ThrowGrenade.Unsubscribe(BasicInteract.Raise);
        MicroUse.Unsubscribe(BasicInteract.Raise);
    }
}

public abstract class BasicItemInteractEvent : PlayerInteractEvent
{
    protected BasicItemInteractEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player) :
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

public class ThrowGrenadeEvent : BasicItemInteractEvent
{
    public ThrowGrenadeEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player, bool throwFullForce) : base(item, state, player)
    {
        ThrowFullForce = throwFullForce;
    }

    public bool ThrowFullForce { get; set; }
}

public class MicroUseEvent : BasicItemInteractEvent
{
    public MicroUseEvent(SynapseItem item, ItemInteractState state, SynapsePlayer player, byte energy,
        bool canScp939Hear, HidState microState) : base(item, state, player)
    {
        Energy = energy;
        CanScp939Hear = canScp939Hear;
        MicroState = microState;
    }

    public byte Energy { get; set; }
    
    public bool CanScp939Hear { get; set; }
    
    public HidState MicroState { get; set; }

    public bool AllowChangingState { get; set; } = true;
}