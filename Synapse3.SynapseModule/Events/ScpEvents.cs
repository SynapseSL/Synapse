using Neuron.Core.Events;
using Neuron.Core.Meta;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public partial class ScpEvents : Service
{
    private readonly EventManager _eventManager;
    private readonly Synapse _synapse;

    public readonly EventReactor<Scp0492AttackEvent> Scp0492Attack = new();
    public readonly EventReactor<Scp049AttackEvent> Scp049Attack = new();
    public readonly EventReactor<Scp049ReviveEvent> Scp049Revive = new();

    public readonly EventReactor<Scp079ContainEvent> Scp079Contain = new();
    public readonly EventReactor<Scp079SwitchCameraEvent> Scp079SwitchCamera = new();
    public readonly EventReactor<Scp079DoorInteractEvent> Scp079DoorInteract = new();
    public readonly EventReactor<Scp079LockDoorEvent> Scp079LockDoor = new();
    public readonly EventReactor<Scp079SpeakerUseEvent> Scp079SpeakerUse = new();

    public readonly EventReactor<Scp096AttackEvent> Scp096Attack = new();
    public readonly EventReactor<Scp096AddTargetEvent> Scp096AddTarget = new();

    public readonly EventReactor<Scp106AttackEvent> Scp106Attack = new();
    public readonly EventReactor<Scp106LeavePocketEvent> Scp106LeavePocket = new();

    public readonly EventReactor<Scp173AttackEvent> Scp173Attack = new();
    public readonly EventReactor<Scp173ObserveEvent> Scp173Observe = new();
    public readonly EventReactor<Scp173PlaceTantrumEvent> Scp173PlaceTantrum = new();
    public readonly EventReactor<Scp173ActivateBreakneckSpeedEvent> Scp173ActivateBreakneckSpeed = new();

    public readonly EventReactor<Scp939AttackEvent> Scp939Attack = new();

    public ScpEvents(EventManager eventManager, Synapse synapse)
    {
        _eventManager = eventManager;
        _synapse = synapse;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Scp0492Attack);
        _eventManager.RegisterEvent(Scp049Attack);
        _eventManager.RegisterEvent(Scp049Revive);

        _eventManager.RegisterEvent(Scp079Contain);
        _eventManager.RegisterEvent(Scp079SwitchCamera);
        _eventManager.RegisterEvent(Scp079DoorInteract);
        _eventManager.RegisterEvent(Scp079LockDoor);
        _eventManager.RegisterEvent(Scp079SpeakerUse);

        _eventManager.RegisterEvent(Scp096Attack);
        _eventManager.RegisterEvent(Scp096AddTarget);

        _eventManager.RegisterEvent(Scp106Attack);
        _eventManager.RegisterEvent(Scp106LeavePocket);

        _eventManager.RegisterEvent(Scp173Attack);
        _eventManager.RegisterEvent(Scp173Observe);
        _eventManager.RegisterEvent(Scp173PlaceTantrum);
        _eventManager.RegisterEvent(Scp173ActivateBreakneckSpeed);

        _eventManager.RegisterEvent(Scp939Attack);
        
        PluginAPI.Events.EventManager.RegisterEvents(_synapse,this);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Scp0492Attack);
        _eventManager.UnregisterEvent(Scp049Attack);
        _eventManager.UnregisterEvent(Scp049Revive);

        _eventManager.UnregisterEvent(Scp079Contain);
        _eventManager.UnregisterEvent(Scp079SwitchCamera);
        _eventManager.UnregisterEvent(Scp079DoorInteract);
        _eventManager.UnregisterEvent(Scp079LockDoor);
        _eventManager.UnregisterEvent(Scp079SpeakerUse);

        _eventManager.UnregisterEvent(Scp096Attack);
        _eventManager.UnregisterEvent(Scp096AddTarget);

        _eventManager.UnregisterEvent(Scp106Attack);
        _eventManager.UnregisterEvent(Scp106LeavePocket);

        _eventManager.UnregisterEvent(Scp173Attack);
        _eventManager.UnregisterEvent(Scp173Observe);
        _eventManager.UnregisterEvent(Scp173PlaceTantrum);
        _eventManager.UnregisterEvent(Scp173ActivateBreakneckSpeed);
        
        _eventManager.UnregisterEvent(Scp939Attack);
    }
}

public abstract class ScpActionEvent : IEvent
{
    public ScpActionEvent(SynapsePlayer scp, bool allow)
    {
        Scp = scp;
        Allow = allow;
    }

    public SynapsePlayer Scp { get; }

    public bool Allow { get; set; }

}

public abstract class ScpAttackEvent : ScpActionEvent
{
    protected ScpAttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage) : base(scp,
        Synapse3Extensions.GetHarmPermission(scp, victim))
    {
        Victim = victim;
        Damage = damage;
    }
    
    public abstract ScpAttackType ScpAttackType { get; }
    
    public SynapsePlayer Victim { get; }
    
    public float Damage { get; set; }
    
}

public class Scp049AttackEvent : ScpAttackEvent
{
    public Scp049AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, 
        float cooldown, bool enableCardiac) : base(
        scp, victim, damage)
    {
        Cooldown = cooldown;
        EnableCardiacEffect = enableCardiac;
    }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp049Touch;
    
    public bool EnableCardiacEffect { get; set; }

    public float Cooldown { get; set; }
}

public class Scp0492AttackEvent : ScpAttackEvent
{
    public Scp0492AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage) : base(scp, victim, damage) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp0492Scratch;
}

public class Scp096AttackEvent : ScpAttackEvent
{
    public Scp096AttackEvent(SynapsePlayer scp, SynapsePlayer victim, Scp096DamageHandler.AttackType attackType,
        float damage) : base(scp, victim, damage)
    {
        ScpAttackType = attackType switch
        {
            Scp096DamageHandler.AttackType.SlapLeft => ScpAttackType.Scp096Slap,
            Scp096DamageHandler.AttackType.SlapRight => ScpAttackType.Scp096Slap,
            Scp096DamageHandler.AttackType.Charge => ScpAttackType.Scp096Charge,
            Scp096DamageHandler.AttackType.GateKill => ScpAttackType.Scp096TearGate,
            _ => ScpAttackType.Scp096Slap,
        };
    }

    public override ScpAttackType ScpAttackType { get; }
}

public class Scp173AttackEvent : ScpAttackEvent
{
    public Scp173AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool tp) :
        base(scp, victim, damage)
    {
        ScpAttackType = tp ? ScpAttackType.Scp173Tp : ScpAttackType.Scp173Snap;
    }

    public override ScpAttackType ScpAttackType { get; }
}

public class Scp939AttackEvent : ScpAttackEvent
{
    public Scp939AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, Scp939DamageType type) : base(scp, victim, damage) 
    {
        switch (type)
        {
            case Scp939DamageType.None:
                ScpAttackType = ScpAttackType.Scp939None;
                    break;
            case Scp939DamageType.Claw:
                ScpAttackType = ScpAttackType.Scp939Claw;
                    break;
            case Scp939DamageType.LungeTarget:
                ScpAttackType = ScpAttackType.Scp939Lunge;
                    break;
            case Scp939DamageType.LungeSecondary:
                ScpAttackType = ScpAttackType.Scp939LungeSecondary;
                break;
            default:
                ScpAttackType = ScpAttackType.Scp939None;
                break;
        }
    }
    
    public override ScpAttackType ScpAttackType { get; }
}

public class Scp106AttackEvent : ScpAttackEvent
{
    public Scp106AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool takeToPocket, float cooldown) : base(scp, victim, damage)
    {
        TakeToPocket = takeToPocket;
        Cooldown = cooldown;
    }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp106Grab;
    
    public bool TakeToPocket { get; set; }
    
    public float Cooldown { get; set; }
}

public class Scp173ObserveEvent : PlayerInteractEvent
{
    public Scp173ObserveEvent(SynapsePlayer player, bool allow, SynapsePlayer scp) : base(player, allow)
    {
        Scp = scp;
    }

    public SynapsePlayer Scp { get; }
}

public class Scp173PlaceTantrumEvent : ScpActionEvent
{
    public Scp173PlaceTantrumEvent(SynapsePlayer scp, float coolDown) : base (scp , true) 
    {
        CoolDown = coolDown;
    }

    public float CoolDown { get; set; }
}

public class Scp173ActivateBreakneckSpeedEvent : ScpActionEvent
{
    public Scp173ActivateBreakneckSpeedEvent(SynapsePlayer scp, bool activate) : base(scp, true)
    {
        Activate = activate;
    }

    public bool Activate { get; }
}

public class Scp049ReviveEvent : ScpActionEvent
{
    public Scp049ReviveEvent(SynapsePlayer scp, SynapsePlayer humanToRevive, SynapseRagDoll ragDoll, bool finishRevive) : base(scp, true)
    {
        HumanToRevive = humanToRevive;
        RagDoll = ragDoll;
        FinishRevive = finishRevive;
    }
    
    public SynapsePlayer HumanToRevive { get; }
    
    public SynapseRagDoll RagDoll { get; }
    
    public bool FinishRevive { get; }

}

public class Scp106LeavePocketEvent : PlayerEvent
{
    public Scp106LeavePocketEvent(SynapsePlayer player, bool escapePocket, 
        Vector3 enteredExitPosition, Vector3 exitPosition) : base(player)
    {
        EscapePocket = escapePocket;
        EnteredExitPosition = enteredExitPosition;
        EscapePosition = exitPosition;
    }
    
    public bool EscapePocket { get; set; }
    
    public Vector3 EnteredExitPosition { get; }

    public Vector3 EscapePosition { get; set; }

    public bool Allow { get; set; } = true;
}

public class Scp096AddTargetEvent : PlayerInteractEvent
{
    public Scp096AddTargetEvent(SynapsePlayer player, bool allow, SynapsePlayer scp, bool isForLooking) : base(player, allow)
    {
        Scp = scp;
        IsForLooking = isForLooking;
    }
    
    public SynapsePlayer Scp { get; }
    
    public bool IsForLooking { get; }
}

public class Scp079ContainEvent : IEvent
{
    public Scp079ContainEvent(Scp079ContainmentStatus status)
    {
        Status = status;
    }

    public Scp079ContainmentStatus Status { get; }

    public bool Allow { get; set; } = true;
}

public class Scp079SwitchCameraEvent : ScpActionEvent
{
    public Scp079SwitchCameraEvent(bool spawning, SynapsePlayer scp, SynapseCamera camera, int cost) 
        : base(scp, true)
    {
        Spawning = spawning;
        Camera = camera;
        Cost = cost;
    }

    public SynapseCamera Camera { get; set; }
    
    public bool Spawning { get; }

    public int Cost { get; set; }
}

public class Scp079DoorInteractEvent : ScpActionEvent
{
    public Scp079DoorInteractEvent(SynapsePlayer scp, SynapseDoor door, int cost) : base(scp, true)
    {
        Door = door;
        Cost = cost;
    }

    public SynapseDoor Door { get; }
    
    public int Cost { get; set; }

}

public class Scp079LockDoorEvent : ScpActionEvent
{
    public Scp079LockDoorEvent(SynapsePlayer scp, SynapseDoor door, bool unlock, int cost) : base(scp, true)
    {
        Door = door;
        Unlock = unlock;
        Cost = cost;
    }

    public SynapseDoor Door { get; }

    public bool Unlock { get; }
    
    public int Cost { get; set; }

}

public class Scp079SpeakerUseEvent : ScpActionEvent
{
    public Scp079SpeakerUseEvent(SynapsePlayer scp, Vector3 speackerPostion) : base(scp, true)
    {
        SpeackerPostion = speackerPostion;
    }

    public Vector3 SpeackerPostion { get; }

}
