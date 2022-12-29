using Neuron.Core.Events;
using Neuron.Core.Meta;
using PlayerRoles.PlayableScps.Scp939;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public partial class ScpEvents : Service
{
    private readonly EventManager _eventManager;
    
    public readonly EventReactor<Scp0492AttackEvent> Scp0492Attack = new();
    public readonly EventReactor<Scp049AttackEvent> Scp049Attack = new();
    public readonly EventReactor<Scp049ReviveEvent> Scp049Revive = new();

    public readonly EventReactor<Scp079ContainEvent> Scp079Contain = new();
    public readonly EventReactor<scp079SwitchCameraEvent> Scp079SwitchCamera = new();
    public readonly EventReactor<Scp079DoorInteractEvent> Scp079DoorInteract = new();
    public readonly EventReactor<Scp079LockDoorEvent> Scp079LockDoor = new();
    public readonly EventReactor<Scp079StartSpeakerEvent> Scp079StartSpeaker = new();
    public readonly EventReactor<Scp079StopSpeakerEvent> Scp079StopSpeaker = new();

    public readonly EventReactor<Scp096AttackEvent> Scp096Attack = new();
    public readonly EventReactor<Scp096ObserveEvent> Scp096Observe = new();

    public readonly EventReactor<Scp106AttackEvent> Scp106Attack = new();
    public readonly EventReactor<Scp106LeavePocketEvent> Scp106LeavePocket = new();

    public readonly EventReactor<Scp173AttackEvent> Scp173Attack = new();
    public readonly EventReactor<Scp173ObserveEvent> Scp173Observe = new();
    public readonly EventReactor<Scp173PlaceTantrumEvent> Scp173PlaceTantrum = new();
    public readonly EventReactor<Scp173ActivateBreakneckSpeedEvent> Scp173ActivateBreakneckSpeed = new();

    public readonly EventReactor<Scp939AttackEvent> Scp939Attack = new();

    public ScpEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
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
        _eventManager.RegisterEvent(Scp079StartSpeaker);
        _eventManager.RegisterEvent(Scp079StopSpeaker);

        _eventManager.RegisterEvent(Scp096Attack);
        _eventManager.RegisterEvent(Scp096Observe);

        _eventManager.RegisterEvent(Scp106Attack);
        _eventManager.RegisterEvent(Scp106LeavePocket);

        _eventManager.RegisterEvent(Scp173Attack);
        _eventManager.RegisterEvent(Scp173Observe);
        _eventManager.RegisterEvent(Scp173PlaceTantrum);
        _eventManager.RegisterEvent(Scp173ActivateBreakneckSpeed);

        _eventManager.RegisterEvent(Scp939Attack);
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
        _eventManager.UnregisterEvent(Scp079StartSpeaker);
        _eventManager.UnregisterEvent(Scp079StopSpeaker);

        _eventManager.UnregisterEvent(Scp096Attack);
        _eventManager.UnregisterEvent(Scp096Observe);

        _eventManager.UnregisterEvent(Scp106Attack);
        _eventManager.UnregisterEvent(Scp106LeavePocket);

        _eventManager.UnregisterEvent(Scp173Attack);
        _eventManager.UnregisterEvent(Scp173Observe);
        _eventManager.UnregisterEvent(Scp173PlaceTantrum);
        
        _eventManager.UnregisterEvent(Scp939Attack);
    }
}

public abstract class ScpAttackEvent : ScpActionEvent
{
    protected ScpAttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, allow)
    {
        Victim = victim;
        Damage = damage;
    }
    
    public abstract ScpAttackType ScpAttackType { get; }
    
    public SynapsePlayer Victim { get; }
    
    public float Damage { get; set; }
    
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

public class Scp049AttackEvent : ScpAttackEvent
{
    public Scp049AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, 
        float cooldown, bool cardiacArrest) : base(
        scp, victim, damage, true)
    {
        Cooldown = cooldown;
        CardiacArrestEffect = cardiacArrest;
    }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp049Touch;
    
    public bool CardiacArrestEffect { get; set; }

    public float Cooldown { get; set; }
}

public class Scp0492AttackEvent : ScpAttackEvent
{
    public Scp0492AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, victim, damage, allow) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp0492Scratch;
}

public class Scp096AttackEvent : ScpAttackEvent
{
    public Scp096AttackEvent(SynapsePlayer scp, SynapsePlayer victim, bool charge, float damage) : base(scp, victim, damage, true) 
    {
        ScpAttackType = charge ? ScpAttackType.Scp096Charge : ScpAttackType.Scp096Tear;
    }

    public override ScpAttackType ScpAttackType { get; }

    //TODO:
    public bool RemoveTarget { get; set; } = true;
}

public class Scp173AttackEvent : ScpAttackEvent
{
    public Scp173AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow) : base(scp, victim, damage, allow) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp173Snap;
}

public class Scp939AttackEvent : ScpAttackEvent
{
    public Scp939AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, Scp939DamageType type) : base(scp, victim, damage, true) 
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
                ScpAttackType = ScpAttackType.Scp939LungeSeconde;
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
    public Scp106AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow, bool takeToPocket) : base(scp, victim, damage, allow)
    {
        TakeToPocket = takeToPocket;
    }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp106Grab;
    
    public bool TakeToPocket { get; set; }
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
    public Scp173PlaceTantrumEvent(SynapsePlayer scp) : base (scp , true) { }

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
    public Scp106LeavePocketEvent(SynapsePlayer player, bool escapePocket, Vector3 enteredPosition) : base(player)
    {
        EscapePocket = escapePocket;
        EnteredPosition = enteredPosition;
    }
    
    public bool EscapePocket { get; set; }
    
    public Vector3 EnteredPosition { get; set; }
}

public class Scp096ObserveEvent : PlayerInteractEvent
{
    public Scp096ObserveEvent(SynapsePlayer player, bool allow, SynapsePlayer scp) : base(player, allow)
    {
        Scp = scp;
    }
    
    public SynapsePlayer Scp { get; }
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

public class scp079SwitchCameraEvent : ScpActionEvent
{
    public scp079SwitchCameraEvent(bool spawning, SynapsePlayer scp, SynapseCamera camera) : base(scp, true)
    {
        Spawning = spawning;
        Camera = camera;
    }

    public SynapseCamera Camera { get; set; }
    
    public bool Spawning { get; }
}

public class Scp079DoorInteractEvent : ScpActionEvent
{
    public Scp079DoorInteractEvent(SynapsePlayer scp, SynapseDoor door, float energy) : base(scp, true)
    {
        Door = door;
        Energy = energy;
    }

    public SynapseDoor Door { get; }
    
    public float Energy { get; set; }

}

public class Scp079LockDoorEvent : ScpActionEvent
{
    public Scp079LockDoorEvent(SynapsePlayer scp, SynapseDoor door, bool unlock, float energy) : base(scp, true)
    {
        Door = door;
        Unlock = unlock;
        Energy = energy;
    }

    public SynapseDoor Door { get; }

    public bool Unlock { get; }
    
    public float Energy { get; set; }

}

public class Scp079StartSpeakerEvent : ScpActionEvent
{
    public Scp079StartSpeakerEvent(SynapsePlayer scp, float energy, string speakerName) : base(scp, true)
    {
        Energy = energy;
        SpeakerName = speakerName;
    }

    public string SpeakerName { get; }

    public float Energy { get; set; }

}

public class Scp079StopSpeakerEvent : ScpActionEvent
{
    public Scp079StopSpeakerEvent(SynapsePlayer scp, string speakerName) : base(scp, true)
    {
        SpeakerName = speakerName;
    }

    public string SpeakerName { get; }
    
}