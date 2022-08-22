using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Events;

public class ScpEvents : Service
{
    private readonly EventManager _eventManager;
    
    public readonly EventReactor<Scp0492AttackEvent> Scp0492Attack = new();
    public readonly EventReactor<Scp049AttackEvent> Scp049Attack = new();
    public readonly EventReactor<Scp096AttackEvent> Scp096Attack = new();
    public readonly EventReactor<Scp173AttackEvent> Scp173Attack = new();
    public readonly EventReactor<Scp939AttackEvent> Scp939Attack = new();
    public readonly EventReactor<Scp106AttackEvent> Scp106Attack = new();
    public readonly EventReactor<ObserveScp173Event> ObserveScp173 = new();
    public readonly EventReactor<ReviveEvent> Revive = new();

    public ScpEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Scp0492Attack);
        _eventManager.RegisterEvent(Scp049Attack);
        _eventManager.RegisterEvent(Scp096Attack);
        _eventManager.RegisterEvent(Scp173Attack);
        _eventManager.RegisterEvent(Scp939Attack);
        _eventManager.RegisterEvent(Scp106Attack);
        _eventManager.RegisterEvent(ObserveScp173);
        _eventManager.RegisterEvent(Revive);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Scp0492Attack);
        _eventManager.UnregisterEvent(Scp049Attack);
        _eventManager.UnregisterEvent(Scp096Attack);
        _eventManager.UnregisterEvent(Scp173Attack);
        _eventManager.UnregisterEvent(Scp939Attack);
        _eventManager.UnregisterEvent(Scp106Attack);
        _eventManager.UnregisterEvent(ObserveScp173);
        _eventManager.UnregisterEvent(Revive);
    }
}

public abstract class ScpAttackEvent : IEvent
{
    protected ScpAttackEvent(SynapsePlayer scp, SynapsePlayer victim,float damage, bool allow)
    {
        Scp = scp;
        Victim = victim;
        Damage = damage;
        Allow = allow;
    }
    
    public abstract ScpAttackType ScpAttackType { get; }
    
    public SynapsePlayer Scp { get; }
    
    public SynapsePlayer Victim { get; }
    
    public float Damage { get; set; }
    
    public bool Allow { get; set; }
}

public class Scp049AttackEvent : ScpAttackEvent
{
    public Scp049AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, float cooldown, bool allow) : base(
        scp, victim, damage, allow)
    {
        Cooldown = cooldown;
    }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp049Touch;
    
    public float Cooldown { get; }
}

public class Scp0492AttackEvent : ScpAttackEvent
{
    public Scp0492AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, victim, damage, allow) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp0492Scratch;
}

public class Scp096AttackEvent : ScpAttackEvent
{
    public Scp096AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, victim, damage, allow) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp096Tear;
}

public class Scp173AttackEvent : ScpAttackEvent
{
    public Scp173AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, victim, damage, allow) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp173Snap;
}

public class Scp939AttackEvent : ScpAttackEvent
{
    public Scp939AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, victim, damage, allow) { }
    
    public override ScpAttackType ScpAttackType => ScpAttackType.Scp939Bite;
}

public class Scp106AttackEvent : ScpAttackEvent
{
    public Scp106AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, victim, damage, allow) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp106Grab;
}

public class ObserveScp173Event : PlayerInteractEvent
{
    public ObserveScp173Event(SynapsePlayer player, bool allow, SynapsePlayer scp173) : base(player, allow)
    {
        Scp173 = scp173;
    }

    public SynapsePlayer Scp173 { get; }
}

public class ReviveEvent : IEvent
{
    public ReviveEvent(SynapsePlayer scp049, SynapsePlayer humanToRevive, SynapseRagdoll ragdoll, bool finishRevive)
    {
        Scp049 = scp049;
        HumanToRevive = humanToRevive;
        Ragdoll = ragdoll;
        FinishRevive = finishRevive;
    }
    
    public SynapsePlayer Scp049 { get; }
    
    public SynapsePlayer HumanToRevive { get; }
    
    public SynapseRagdoll Ragdoll { get; }
    
    public bool FinishRevive { get; }

    public bool Allow { get; set; } = true;
}