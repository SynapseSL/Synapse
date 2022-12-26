using Neuron.Core.Events;
using Neuron.Core.Meta;
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
    public readonly EventReactor<Scp106ContainEvent> Scp106Contain = new();
    public readonly EventReactor<Scp106CreatePortalEvent> Scp106CreatePortal = new();
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
        _eventManager.RegisterEvent(Scp106Contain);
        _eventManager.RegisterEvent(Scp106CreatePortal);
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
        _eventManager.UnregisterEvent(Scp106Contain);
        _eventManager.UnregisterEvent(Scp106CreatePortal);
        _eventManager.UnregisterEvent(Scp106LeavePocket);

        _eventManager.UnregisterEvent(Scp173Attack);
        _eventManager.UnregisterEvent(Scp173Observe);
        _eventManager.UnregisterEvent(Scp173PlaceTantrum);
        
        _eventManager.UnregisterEvent(Scp939Attack);
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
    public Scp096AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage) : base(scp, victim, damage, true) { }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp096Tear;

    public bool RemoveTarget { get; set; } = true;
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
    public Scp106AttackEvent(SynapsePlayer scp, SynapsePlayer victim, float damage, bool allow, bool takeToPocket) : base(scp, victim, damage, allow)
    {
        TakeToPocket = takeToPocket;
    }

    public override ScpAttackType ScpAttackType => ScpAttackType.Scp106Grab;
    
    public bool TakeToPocket { get; set; }
}

public class Scp173ObserveEvent : PlayerInteractEvent
{
    public Scp173ObserveEvent(SynapsePlayer player, bool allow, SynapsePlayer scp173) : base(player, allow)
    {
        Scp173 = scp173;
    }

    public SynapsePlayer Scp173 { get; }
}

public class Scp173PlaceTantrumEvent : IEvent
{
    public Scp173PlaceTantrumEvent(SynapsePlayer scp173)
    {
        Scp173 = scp173;
    }

    public SynapsePlayer Scp173 { get; }

    public bool Allow { get; set; } = true;
}

public class Scp173ActivateBreakneckSpeedEvent : IEvent
{
    public Scp173ActivateBreakneckSpeedEvent(SynapsePlayer scp173, bool activate)
    {
        Scp173 = scp173;
        Activate = activate;
    }

    public bool Activate { get; }

    public SynapsePlayer Scp173 { get; }

    public bool Allow { get; set; } = true;
}

public class Scp049ReviveEvent : IEvent
{
    public Scp049ReviveEvent(SynapsePlayer scp049, SynapsePlayer humanToRevive, SynapseRagDoll ragDoll, bool finishRevive)
    {
        Scp049 = scp049;
        HumanToRevive = humanToRevive;
        RagDoll = ragDoll;
        FinishRevive = finishRevive;
    }
    
    public SynapsePlayer Scp049 { get; }
    
    public SynapsePlayer HumanToRevive { get; }
    
    public SynapseRagDoll RagDoll { get; }
    
    public bool FinishRevive { get; }

    public bool Allow { get; set; } = true;
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
    public Scp096ObserveEvent(SynapsePlayer player, bool allow, SynapsePlayer scp096) : base(player, allow)
    {
        Scp096 = scp096;
    }
    
    public SynapsePlayer Scp096 { get; }
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

public class scp079SwitchCameraEvent : IEvent
{
    public scp079SwitchCameraEvent(bool spawning, SynapsePlayer scp079, SynapseCamera camera)
    {
        Spawning = spawning;
        Scp079 = scp079;
        Camera = camera;
    }

    public SynapseCamera Camera { get; set; }
    
    public SynapsePlayer Scp079 { get; }

    public bool Allow { get; set; } = true;
    
    public bool Spawning { get; }
}

public class Scp079DoorInteractEvent : IEvent
{
    public Scp079DoorInteractEvent(SynapsePlayer scp079, SynapseDoor door, float energy)
    {
        Scp079 = scp079;
        Door = door;
        Energy = energy;
    }

    public SynapsePlayer Scp079 { get; }
    
    public SynapseDoor Door { get; }
    
    public float Energy { get; set; }

    public bool Allow { get; set; } = true;
}

public class Scp079LockDoorEvent : IEvent
{
    public Scp079LockDoorEvent(SynapsePlayer scp079, SynapseDoor door, bool unlock, float energy)
    {
        Scp079 = scp079;
        Door = door;
        Unlock = unlock;
        Energy = energy;
    }

    public SynapsePlayer Scp079 { get; }
    
    public SynapseDoor Door { get; }

    public bool Unlock { get; }
    
    public float Energy { get; set; }

    public bool Allow { get; set; } = true;
}

public class Scp079StartSpeakerEvent : IEvent
{
    public Scp079StartSpeakerEvent(SynapsePlayer scp079, float energy, string speakerName)
    {
        Scp079 = scp079;
        Energy = energy;
        SpeakerName = speakerName;
    }

    public SynapsePlayer Scp079 { get; }
    
    public string SpeakerName { get; }

    public float Energy { get; set; }

    public bool Allow { get; set; } = true;
}

public class Scp079StopSpeakerEvent : IEvent
{
    public Scp079StopSpeakerEvent(SynapsePlayer scp079, string speakerName)
    {
        Scp079 = scp079;
        SpeakerName = speakerName;
    }

    public SynapsePlayer Scp079 { get; }

    public string SpeakerName { get; }
    
    public bool Allow { get; set; } = true;
}