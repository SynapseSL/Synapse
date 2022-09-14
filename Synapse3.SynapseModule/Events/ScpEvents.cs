using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;

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
    public readonly EventReactor<PlaceTantrumEvent> PlaceTantrum = new();
    public readonly EventReactor<ActivateBreakneckSpeedEvent> ActivateBreakneckSpeed = new();
    public readonly EventReactor<ReviveEvent> Revive = new();
    public readonly EventReactor<ContainScp106Event> ContainScp106 = new();
    public readonly EventReactor<CreatePortalEvent> CreatePortal = new();
    public readonly EventReactor<LeavePocketEvent> LeavePocket = new();
    public readonly EventReactor<ObserveScp096Event> ObserveScp096 = new();
    public readonly EventReactor<ContainScp079Event> ContainScp079 = new();
    public readonly EventReactor<SwitchCameraEvent> SwitchCamera = new();
    public readonly EventReactor<Scp079DoorInteractEvent> Scp079DoorInteract = new();
    public readonly EventReactor<Scp079LockDoorEvent> Scp079LockDoor = new();
    public readonly EventReactor<Scp079StartSpeakerEvent> Scp079StartSpeaker = new();
    public readonly EventReactor<Scp079StopSpeakerEvent> Scp079StopSpeaker = new();

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
        _eventManager.RegisterEvent(PlaceTantrum);
        _eventManager.RegisterEvent(ActivateBreakneckSpeed);
        _eventManager.RegisterEvent(Revive);
        _eventManager.RegisterEvent(ContainScp106);
        _eventManager.RegisterEvent(CreatePortal);
        _eventManager.RegisterEvent(LeavePocket);
        _eventManager.RegisterEvent(ObserveScp096);
        _eventManager.RegisterEvent(ContainScp079);
        _eventManager.RegisterEvent(SwitchCamera);
        _eventManager.RegisterEvent(Scp079DoorInteract);
        _eventManager.RegisterEvent(Scp079LockDoor);
        _eventManager.RegisterEvent(Scp079StartSpeaker);
        _eventManager.RegisterEvent(Scp079StopSpeaker);
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
        _eventManager.UnregisterEvent(ContainScp106);
        _eventManager.UnregisterEvent(CreatePortal);
        _eventManager.UnregisterEvent(LeavePocket);
        _eventManager.UnregisterEvent(ObserveScp096);
        _eventManager.UnregisterEvent(ContainScp079);
        _eventManager.UnregisterEvent(SwitchCamera);
        _eventManager.UnregisterEvent(Scp079DoorInteract);
        _eventManager.UnregisterEvent(Scp079LockDoor);
        _eventManager.UnregisterEvent(Scp079StartSpeaker);
        _eventManager.UnregisterEvent(Scp079StopSpeaker);
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

public class ObserveScp173Event : PlayerInteractEvent
{
    public ObserveScp173Event(SynapsePlayer player, bool allow, SynapsePlayer scp173) : base(player, allow)
    {
        Scp173 = scp173;
    }

    public SynapsePlayer Scp173 { get; }
}

public class PlaceTantrumEvent : IEvent
{
    public PlaceTantrumEvent(SynapsePlayer scp173)
    {
        Scp173 = scp173;
    }

    public SynapsePlayer Scp173 { get; }

    public bool Allow { get; set; } = true;
}

public class ActivateBreakneckSpeedEvent : IEvent
{
    public ActivateBreakneckSpeedEvent(SynapsePlayer scp173)
    {
        Scp173 = scp173;
    }

    public SynapsePlayer Scp173 { get; }

    public bool Allow { get; set; } = true;
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

public class ContainScp106Event : PlayerInteractEvent
{
    public ContainScp106Event(SynapsePlayer player) : base(player, true) { }
}

public class CreatePortalEvent : IEvent
{
    public CreatePortalEvent(SynapsePlayer scp106, Vector3 position)
    {
        Scp106 = scp106;
        Position = position;
    }

    public SynapsePlayer Scp106 { get; }
    
    public Vector3 Position { get; set; }
    
    public bool Allow { get; set; }
}

public class LeavePocketEvent : PlayerEvent
{
    public LeavePocketEvent(SynapsePlayer player, bool escapePocket, Vector3 enteredPosition) : base(player)
    {
        EscapePocket = escapePocket;
        EnteredPosition = enteredPosition;
    }
    
    public bool EscapePocket { get; set; }
    
    public Vector3 EnteredPosition { get; set; }
}

public class ObserveScp096Event : PlayerInteractEvent
{
    public ObserveScp096Event(SynapsePlayer player, bool allow, SynapsePlayer scp096) : base(player, allow)
    {
        Scp096 = scp096;
    }
    
    public SynapsePlayer Scp096 { get; }
}

public class ContainScp079Event : IEvent
{
    public ContainScp079Event(Scp079ContainmentStatus status)
    {
        Status = status;
    }

    public Scp079ContainmentStatus Status { get; }

    public bool Allow { get; set; } = true;
}

public class SwitchCameraEvent : IEvent
{
    public SwitchCameraEvent(bool spawning, SynapsePlayer scp079, SynapseCamera camera)
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