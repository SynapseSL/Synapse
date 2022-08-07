﻿using System.Collections.Generic;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public class PlayerEvents : Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<LoadComponentEvent> LoadComponent = new();
    public readonly EventReactor<KeyPressEvent> KeyPress = new();
    public readonly EventReactor<HarmPermissionEvent> HarmPermission = new();
    public readonly EventReactor<SetClassEvent> SetClass = new();
    public readonly EventReactor<UpdateEvent> Update = new();
    public readonly EventReactor<DoorInteractEvent> DoorInteract = new();
    public readonly EventReactor<LockerUseEvent> LockerUse = new();
    public readonly EventReactor<StartWarheadEvent> StartWarhead = new();
    public readonly EventReactor<WarheadPanelInteractEvent> WarheadPanelInteract = new();
    public readonly EventReactor<BanEvent> Ban = new();
    public readonly EventReactor<DamageEvent> Damage = new();
    public readonly EventReactor<DeathEvent> Death = new();
    public readonly EventReactor<FreePlayerEvent> FreePlayer = new();
    public readonly EventReactor<DropAmmoEvent> DropAmmo = new();
    public readonly EventReactor<EscapeEvent> Escape = new();
    public readonly EventReactor<ChangeItemEvent> ChangeItem = new();
    public readonly EventReactor<PickupEvent> Pickup = new();
    public readonly EventReactor<DropItemEvent> DropItem = new();
    public readonly EventReactor<EnterFemurEvent> EnterFemur = new();
    public readonly EventReactor<GeneratorInteractEvent> GeneratorInteract = new();
    public readonly EventReactor<HealEvent> Heal = new();
    public readonly EventReactor<JoinEvent> Join = new();
    public readonly EventReactor<LeaveEvent> Leave = new();
    public readonly EventReactor<PlaceBulletHoleEvent> PlaceBulletHole = new();
    public readonly EventReactor<ReportEvent> Report = new();
    public readonly EventReactor<SpeakSecondaryEvent> SpeakSecondary = new();
    public readonly EventReactor<OpenWarheadButtonEvent> OpenWarheadButton = new();
    public readonly EventReactor<WalkOnHazardEvent> WalkOnHazard = new();
    public readonly EventReactor<WalkOnSinkholeEvent> WalkOnSinkhole = new();
    public readonly EventReactor<WalkOnTantrumEvent> WalkOnTantrum = new();
    public readonly EventReactor<StartWorkStationEvent> StartWorkStation = new();
    public readonly EventReactor<FallingIntoAbyssEvent> FallingIntoAbyss = new();

    public PlayerEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(LoadComponent);
        _eventManager.RegisterEvent(KeyPress);
        _eventManager.RegisterEvent(HarmPermission);
        _eventManager.RegisterEvent(SetClass);
        _eventManager.RegisterEvent(StartWarhead);
        _eventManager.RegisterEvent(DoorInteract);
        _eventManager.RegisterEvent(LockerUse);
        _eventManager.RegisterEvent(WarheadPanelInteract);
        _eventManager.RegisterEvent(Ban);
        _eventManager.RegisterEvent(ChangeItem);
        _eventManager.RegisterEvent(Death);
        _eventManager.RegisterEvent(FreePlayer);
        _eventManager.RegisterEvent(DropAmmo);
        _eventManager.RegisterEvent(Escape);
        _eventManager.RegisterEvent(DropItem);
        _eventManager.RegisterEvent(EnterFemur);
        _eventManager.RegisterEvent(GeneratorInteract);
        _eventManager.RegisterEvent(Heal);
        _eventManager.RegisterEvent(Join);
        _eventManager.RegisterEvent(Leave);
        _eventManager.RegisterEvent(Pickup);
        _eventManager.RegisterEvent(PlaceBulletHole);
        _eventManager.RegisterEvent(Report);
        _eventManager.RegisterEvent(SpeakSecondary);
        _eventManager.RegisterEvent(OpenWarheadButton);
        _eventManager.RegisterEvent(StartWorkStation);
        _eventManager.RegisterEvent(FallingIntoAbyss);

        WalkOnSinkhole.Subscribe(ev => WalkOnHazard.Raise(ev));
        WalkOnTantrum.Subscribe(ev => WalkOnHazard.Raise(ev));
    }
}

public abstract class PlayerEvent : IEvent
{
    public SynapsePlayer Player { get; }

    protected PlayerEvent(SynapsePlayer player)
    {
        Player = player;
    }
}

public abstract class PlayerInteractEvent : PlayerEvent
{
    public bool Allow { get; set; }

    protected PlayerInteractEvent(SynapsePlayer player, bool allow) : base(player)
    {
        Allow = allow;
    }
} 

public class LoadComponentEvent : PlayerEvent
{
    public LoadComponentEvent(GameObject game,SynapsePlayer player) : base(player)
    {
        PlayerGameObject = game;
    }

    public GameObject PlayerGameObject { get; }

    public TComponent AddComponent<TComponent>() where TComponent : Component
    {
        var comp = (TComponent)PlayerGameObject.GetComponent(typeof(TComponent));
        if (comp == null)
            return PlayerGameObject.AddComponent<TComponent>();

        return comp;
    }
}

public class KeyPressEvent : PlayerEvent
{
    public KeyCode KeyCode { get; }

    public KeyPressEvent(SynapsePlayer player, KeyCode keyCode) : base(player)
    {
        KeyCode = keyCode;
    }
}

public class HarmPermissionEvent : IEvent
{
    public HarmPermissionEvent(SynapsePlayer attacker, SynapsePlayer victim, bool allow)
    {
        Victim = victim;
        Attacker = attacker;
        Allow = allow;
    }
    
    public bool Allow { get; set; }
    
    public SynapsePlayer Attacker { get; }
    
    public SynapsePlayer Victim { get; }
}

public class SetClassEvent : PlayerInteractEvent
{
    public SetClassEvent(SynapsePlayer player, RoleType role, CharacterClassManager.SpawnReason reason) : base(player,
        true)
    {
        Role = role;
        SpawnReason = reason;
    }
    
    public RoleType Role { get; set; }
    
    public CharacterClassManager.SpawnReason SpawnReason { get; }

    public List<uint> Items { get; set; } = new();

    public List<SynapseItem> EscapeItems { get; set; } = new();
    
    public Vector3 Position { get; set; } = Vector3.zero;

    public PlayerMovementSync.PlayerRotation Rotation { get; set; }

    public Dictionary<AmmoType, ushort> Ammo { get; set; } = new();
}

public class UpdateEvent : PlayerEvent
{
    public UpdateEvent(SynapsePlayer player) : base(player) { }
}

public class DoorInteractEvent : PlayerInteractEvent
{
    public SynapseDoor Door { get; }
    
    /// <summary>
    /// This is true when a player tries to open/close a locked door and he is not in Bypass or something else causes to overrides the lock like the Nuke
    /// </summary>
    public bool LockBypassRejected { get; }

    public DoorInteractEvent(SynapsePlayer player, bool allow, SynapseDoor door, bool lockBypass) : base(player, allow)
    {
        Door = door;
        LockBypassRejected = lockBypass;
    }
}

public class LockerUseEvent : PlayerInteractEvent
{
    public LockerUseEvent(SynapsePlayer player, bool allow, SynapseLocker locker,
        SynapseLocker.SynapseLockerChamber chamber) : base(player, allow)
    {
        Locker = locker;
        Chamber = chamber;
    }

    public SynapseLocker Locker { get; }
    
    public SynapseLocker.SynapseLockerChamber Chamber { get; }
}

public class StartWarheadEvent : PlayerInteractEvent
{
    public StartWarheadEvent(SynapsePlayer player, bool allow) : base(player, allow) { }
}

public class WarheadPanelInteractEvent : PlayerInteractEvent
{
    public PlayerInteract.AlphaPanelOperations Operation { get; set; }

    public WarheadPanelInteractEvent(SynapsePlayer player, bool allow, PlayerInteract.AlphaPanelOperations operation) :
        base(player, allow)
    {
        Operation = operation;
    }
}

public class BanEvent : PlayerInteractEvent
{
    public BanEvent(SynapsePlayer player, bool allow, SynapsePlayer banIssuer, string reason, long duration,
        bool global) : base(player, allow)
    {
        BanIssuer = banIssuer;
        Reason = reason;
        Duration = duration;
        GlobalBan = global;
    }

    public SynapsePlayer BanIssuer { get; }
    
    public string Reason { get; set; }
    
    public long Duration { get; set; }
    
    public bool GlobalBan { get; }
}

public class ChangeItemEvent : PlayerInteractEvent
{
    public ChangeItemEvent(SynapsePlayer player, bool allow, SynapseItem newItem) : base(player, allow)
    {
        NewItem = newItem;
    }

    public SynapseItem PreviousItem => Player.Inventory.ItemInHand;
    
    public SynapseItem NewItem { get; }
}

public class DamageEvent : PlayerInteractEvent
{
    public DamageEvent(SynapsePlayer player, bool allow, SynapsePlayer attacker, DamageType damageType, float damage) :
        base(player, allow)
    {
        Attacker = attacker;
        DamageType = damageType;
        Damage = damage;
    }

    public SynapsePlayer Attacker { get; }
    
    public DamageType DamageType { get; }
    
    public float Damage { get; set; }
}

public class DeathEvent : PlayerInteractEvent
{
    public DeathEvent(SynapsePlayer player, bool allow, SynapsePlayer attacker, DamageType damageType, float lastTakenDamage) : base(player, allow)
    {
        Attacker = attacker;
        DamageType = damageType;
        LastTakenDamage = lastTakenDamage;
    }

    public SynapsePlayer Attacker { get; }
    
    public DamageType DamageType { get; }
    
    public float LastTakenDamage { get; }
}

public class FreePlayerEvent : PlayerInteractEvent
{
    public FreePlayerEvent(SynapsePlayer player, bool allow, SynapsePlayer disarmedPlayer) : base(player, allow)
    {
        DisarmedPlayer = disarmedPlayer;
    }

    public SynapsePlayer DisarmedPlayer { get; }
}

public class DropAmmoEvent : PlayerInteractEvent
{
    public DropAmmoEvent(SynapsePlayer player, bool allow, AmmoType ammoType, ushort amount) : base(player, allow)
    {
        AmmoType = ammoType;
        Amount = amount;
    }

    public AmmoType AmmoType { get; set; }
    
    public ushort Amount { get; set; }
}

public class EscapeEvent : PlayerInteractEvent
{
    public EscapeEvent(SynapsePlayer player, bool allow, uint newRole, bool isClassD, bool changeTeam) : base(player, allow)
    {
        NewRole = newRole;
        IsClassD = isClassD;
        ChangeTeam = changeTeam;
    }

    public uint NewRole { get; set; }
    
    public bool IsClassD { get; }
    
    public bool ChangeTeam { get; }
}

public class DropItemEvent : PlayerInteractEvent
{
    public DropItemEvent(SynapsePlayer player, bool allow, SynapseItem itemToDrop, bool @throw) : base(player, allow)
    {
        ItemToDrop = itemToDrop;
        Throw = @throw;
    }

    public SynapseItem ItemToDrop { get; }
    
    public bool Throw { get; set; }
}

public class EnterFemurEvent : PlayerInteractEvent
{
    public EnterFemurEvent(SynapsePlayer player, bool allow, bool closeFemur) : base(player, allow)
    {
        CloseFemur = closeFemur;
    }

    public bool CloseFemur { get; set; }
}

public class GeneratorInteractEvent : PlayerInteractEvent
{
    public GeneratorInteractEvent(SynapsePlayer player, bool allow, SynapseGenerator generator, GeneratorInteract interactionType) : base(player, allow)
    {
        Generator = generator;
        InteractionType = interactionType;
    }

    public SynapseGenerator Generator { get; }
    
    public GeneratorInteract InteractionType { get; }
}

public class HealEvent : PlayerInteractEvent
{
    public HealEvent(SynapsePlayer player, bool allow, float amount) : base(player, allow)
    {
        Amount = amount;
    }

    public float Amount { get; set; }
}

public class JoinEvent : PlayerEvent
{
    public JoinEvent(SynapsePlayer player, string nickName) : base(player)
    {
        NickName = nickName;
    }

    public string NickName { get; set; }
}

public class LeaveEvent : PlayerEvent
{
    public LeaveEvent(SynapsePlayer player) : base(player) { }
}

public class PickupEvent : PlayerInteractEvent
{
    public PickupEvent(SynapsePlayer player, bool allow, SynapseItem item) : base(player, allow)
    {
        Item = item;
    }

    public SynapseItem Item { get; }
}

public class PlaceBulletHoleEvent : PlayerInteractEvent
{
    public PlaceBulletHoleEvent(SynapsePlayer player, bool allow, Vector3 position) : base(player, allow)
    {
        Position = position;
    }

    public Vector3 Position { get; }
}

public class ReportEvent : PlayerInteractEvent
{
    public ReportEvent(SynapsePlayer player, bool allow, SynapsePlayer reportedPlayer, string reason, bool sendToNorthWood) : base(player, allow)
    {
        ReportedPlayer = reportedPlayer;
        Reason = reason;
        SendToNorthWood = sendToNorthWood;
    }

    public SynapsePlayer ReportedPlayer { get; }
    
    public string Reason { get; set; }
    
    public bool SendToNorthWood { get; set; }
}

public class SpeakSecondaryEvent : PlayerInteractEvent
{
    public SpeakSecondaryEvent(SynapsePlayer player, bool allow, bool radioChat, bool scp939Chat, bool startSpeaking) : base(player, allow)
    {
        RadioChat = radioChat;
        Scp939Chat = scp939Chat;
        StartSpeaking = startSpeaking;
    }
    
    public bool RadioChat { get; set; }
    public bool Scp939Chat { get; set; }

    public bool StartSpeaking { get; }
}

public class OpenWarheadButtonEvent : PlayerInteractEvent
{
    public OpenWarheadButtonEvent(SynapsePlayer player, bool allow, bool open) : base(player, allow)
    {
        Open = open;
    }

    public bool Open { get; set; }
}

public abstract class WalkOnHazardEvent : PlayerInteractEvent
{
    protected WalkOnHazardEvent(SynapsePlayer player, bool allow, EnvironmentalHazard hazard) : base(player, allow)
    {
        Hazard = hazard;
    }

    public EnvironmentalHazard Hazard { get; }
}

public class WalkOnSinkholeEvent : WalkOnHazardEvent
{
    public WalkOnSinkholeEvent(SynapsePlayer player, bool allow, SinkholeEnvironmentalHazard hazard) : base(player,
        allow, hazard)
    {
        Hazard = hazard;
    }
    
    public new SinkholeEnvironmentalHazard Hazard { get; }
}

public class WalkOnTantrumEvent : WalkOnHazardEvent
{
    public WalkOnTantrumEvent(SynapsePlayer player, bool allow, TantrumEnvironmentalHazard hazard) : base(player, allow, hazard)
    {
        Hazard = hazard;
    }
    
    public new TantrumEnvironmentalHazard Hazard { get; }
}

public class StartWorkStationEvent : PlayerInteractEvent
{
    public StartWorkStationEvent(SynapsePlayer player, bool allow, SynapseWorkStation workStation) : base(player, allow)
    {
        WorkStation = workStation;
    }

    public SynapseWorkStation WorkStation { get; }
}

public class FallingIntoAbyssEvent : PlayerEvent
{
    public FallingIntoAbyssEvent(SynapsePlayer player, bool allow) : base(player)
    {
        Allow = allow;
    }
    public bool Allow { get; set; }
    public SynapsePlayer Player { get; }
}

