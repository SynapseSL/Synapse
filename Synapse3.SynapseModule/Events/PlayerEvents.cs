﻿using System.Collections.Generic;
using Hazards;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using PlayerRoles;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using VoiceChat;

namespace Synapse3.SynapseModule.Events;

public partial class PlayerEvents : Service
{
    private readonly Synapse _synapse;
    private readonly EventManager _eventManager;
    private readonly ItemService _item;

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
    public readonly EventReactor<GeneratorInteractEvent> GeneratorInteract = new();
    public readonly EventReactor<HealEvent> Heal = new();
    public readonly EventReactor<JoinEvent> Join = new();
    public readonly EventReactor<LeaveEvent> Leave = new();
    public readonly EventReactor<PlaceBulletHoleEvent> PlaceBulletHole = new();
    public readonly EventReactor<ReportEvent> Report = new();
    public readonly EventReactor<OpenWarheadButtonEvent> OpenWarheadButton = new();
    public readonly EventReactor<WalkOnHazardEvent> WalkOnHazard = new();
    public readonly EventReactor<WalkOnSinkholeEvent> WalkOnSinkhole = new();
    public readonly EventReactor<WalkOnTantrumEvent> WalkOnTantrum = new();
    public readonly EventReactor<StartWorkStationEvent> StartWorkStation = new();
    public readonly EventReactor<FallingIntoAbyssEvent> FallingIntoAbyss = new();
    public readonly EventReactor<SimpleSetClassEvent> SimpleSetClass = new();
    public readonly EventReactor<UpdateDisplayNameEvent> UpdateDisplayName = new();
    public readonly EventReactor<CheckKeyCardPermissionEvent> CheckKeyCardPermission = new();
    public readonly EventReactor<CallVanillaElevatorEvent> CallVanillaElevator = new();
    public readonly EventReactor<SendPlayerDataEvent> SendPlayerData = new();
    public readonly EventReactor<ChangeRoleEvent> ChangeRole = new();
    public readonly EventReactor<KickEvent> Kick = new();
    public readonly EventReactor<SpeakEvent> Speak = new();
    public readonly EventReactor<SpeakToPlayerEvent> SpeakToPlayer = new();

    public PlayerEvents(EventManager eventManager, Synapse synapse, ItemService item)
    {
        _eventManager = eventManager;
        _synapse = synapse;
        _item = item;
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
        _eventManager.RegisterEvent(GeneratorInteract);
        _eventManager.RegisterEvent(Heal);
        _eventManager.RegisterEvent(Join);
        _eventManager.RegisterEvent(Leave);
        _eventManager.RegisterEvent(Pickup);
        _eventManager.RegisterEvent(PlaceBulletHole);
        _eventManager.RegisterEvent(Report);
        _eventManager.RegisterEvent(OpenWarheadButton);
        _eventManager.RegisterEvent(StartWorkStation);
        _eventManager.RegisterEvent(FallingIntoAbyss);
        _eventManager.RegisterEvent(SimpleSetClass);
        _eventManager.RegisterEvent(UpdateDisplayName);
        _eventManager.RegisterEvent(CheckKeyCardPermission);
        _eventManager.RegisterEvent(CallVanillaElevator);
        _eventManager.RegisterEvent(SendPlayerData);
        _eventManager.RegisterEvent(ChangeRole);
        _eventManager.RegisterEvent(Kick);
        _eventManager.RegisterEvent(Speak);
        _eventManager.RegisterEvent(SpeakToPlayer);

        WalkOnSinkhole.Subscribe(WalkOnHazard.Raise);
        WalkOnTantrum.Subscribe(WalkOnHazard.Raise);
        
        PlayerRoleManager.OnServerRoleSet += CallSimpleSetClass;
        
        PluginAPI.Events.EventManager.RegisterEvents(_synapse,this);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(LoadComponent);
        _eventManager.UnregisterEvent(KeyPress);
        _eventManager.UnregisterEvent(HarmPermission);
        _eventManager.UnregisterEvent(SetClass);
        _eventManager.UnregisterEvent(StartWarhead);
        _eventManager.UnregisterEvent(DoorInteract);
        _eventManager.UnregisterEvent(LockerUse);
        _eventManager.UnregisterEvent(WarheadPanelInteract);
        _eventManager.UnregisterEvent(Ban);
        _eventManager.UnregisterEvent(ChangeItem);
        _eventManager.UnregisterEvent(Death);
        _eventManager.UnregisterEvent(FreePlayer);
        _eventManager.UnregisterEvent(DropAmmo);
        _eventManager.UnregisterEvent(Escape);
        _eventManager.UnregisterEvent(DropItem);
        _eventManager.UnregisterEvent(GeneratorInteract);
        _eventManager.UnregisterEvent(Heal);
        _eventManager.UnregisterEvent(Join);
        _eventManager.UnregisterEvent(Leave);
        _eventManager.UnregisterEvent(Pickup);
        _eventManager.UnregisterEvent(PlaceBulletHole);
        _eventManager.UnregisterEvent(Report);
        _eventManager.UnregisterEvent(OpenWarheadButton);
        _eventManager.UnregisterEvent(StartWorkStation);
        _eventManager.UnregisterEvent(FallingIntoAbyss);
        _eventManager.UnregisterEvent(SimpleSetClass);
        _eventManager.UnregisterEvent(UpdateDisplayName);
        _eventManager.UnregisterEvent(CheckKeyCardPermission);
        _eventManager.UnregisterEvent(CallVanillaElevator);
        _eventManager.UnregisterEvent(SendPlayerData);
        _eventManager.UnregisterEvent(ChangeRole);
        _eventManager.UnregisterEvent(Kick);
        _eventManager.UnregisterEvent(Speak);
        _eventManager.UnregisterEvent(SpeakToPlayer);

        WalkOnSinkhole.Unsubscribe(WalkOnHazard.Raise);
        WalkOnTantrum.Unsubscribe(WalkOnHazard.Raise);

        PlayerRoleManager.OnServerRoleSet -= CallSimpleSetClass;
    }

    private void CallSimpleSetClass(ReferenceHub hub, RoleTypeId newRole, RoleChangeReason reason)
    {
        var player = hub.GetSynapsePlayer();
        if (player == null) return;
        var currentRole = player.RoleType;
        var ev = new SimpleSetClassEvent(player, currentRole, newRole);
        SimpleSetClass.Raise(ev);
        
        switch (currentRole)
        {
            case RoleTypeId.Scp106:
                player.MainScpController.Scp106.ResetDefault();
                break;
            case RoleTypeId.Scp173:
                player.MainScpController.Scp173.ResetDefault();
                break;
        }

        if (player.CustomRole == null)
            Timing.CallDelayed(Timing.WaitForOneFrame,
                () => ChangeRole.Raise(new ChangeRoleEvent(player) { RoleId = (uint)newRole }));
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
    public LoadComponentEvent(GameObject game, SynapsePlayer player) : base(player)
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
    public SetClassEvent(SynapsePlayer player, RoleTypeId role, RoleChangeReason reason, RoleSpawnFlags spawnFlags) :
        base(player,true)
    {
        Role = role;
        SpawnReason = reason;
        SpawnFlags = spawnFlags;
    }

    public RoleTypeId Role { get; set; }

    public RoleChangeReason SpawnReason { get; set; }
    
    public RoleSpawnFlags SpawnFlags { get; set; }
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
    public bool LockBypassRejected { get; set; }

    public bool PlayDeniedSound { get; set; } = true;

    public DoorInteractEvent(SynapsePlayer player, bool allow, SynapseDoor door, bool lockBypass) : base(player, allow)
    {
        Door = door;
        LockBypassRejected = lockBypass;
    }
}

public class LockerUseEvent : PlayerInteractEvent
{
    public LockerUseEvent(SynapsePlayer player, bool hasPerm, SynapseLocker locker,
        SynapseLocker.SynapseLockerChamber chamber) : base(player, true)
    {
        Locker = locker;
        Chamber = chamber;
        IsAllowedToOpen = hasPerm;
    }
    
    public bool IsAllowedToOpen { get; set; }

    public SynapseLocker Locker { get; }

    public SynapseLocker.SynapseLockerChamber Chamber { get; }
}

public class StartWarheadEvent : PlayerInteractEvent
{
    public StartWarheadEvent(SynapsePlayer player, bool allow, bool isResumed) : base(player, allow)
    {
        IsResumed = isResumed;
    }

    public bool IsResumed { get; }
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

public class BanEvent : KickEvent
{
    public BanEvent(SynapsePlayer player, bool allow, SynapsePlayer admin, string reason, long duration) : base(player, admin, reason, allow)
    {
        Duration = duration;
    }

    public long Duration { get; set; }
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
    public DeathEvent(SynapsePlayer player, bool allow, SynapsePlayer attacker, DamageType damageType,
        float lastTakenDamage, string playerMessage, string ragDollMessage) : base(player, allow)
    {
        Attacker = attacker;
        DamageType = damageType;
        LastTakenDamage = lastTakenDamage;
        DeathMessage = playerMessage;
        RagDollInfo = ragDollMessage;
    }

    public SynapsePlayer Attacker { get; }

    public DamageType DamageType { get; }

    public float LastTakenDamage { get; }

    public string DeathMessage { get; set; }

    public string RagDollInfo { get; set; }
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
    public DropAmmoEvent(SynapsePlayer player, bool allow, AmmoType ammoType, ushort amount, bool checkMinimals) : base(player, allow)
    {
        AmmoType = ammoType;
        Amount = amount;
        CheckMinimals = checkMinimals;
    }

    public AmmoType AmmoType { get; set; }

    public ushort Amount { get; set; }
    
    public bool CheckMinimals { get; set; }
}

public class EscapeEvent : PlayerInteractEvent
{
    public EscapeEvent(SynapsePlayer player, bool allow, EscapeType type) : base(player, allow)
    {
        EscapeType = type;
    }
    
    public EscapeType EscapeType { get; set; }

    private uint _role;
    public uint OverrideRole
    {
        get => _role;
        set
        {
            EscapeType = EscapeType.PluginOverride;
            _role = value;
        }
    }
}

public class DropItemEvent : PlayerInteractEvent
{
    public DropItemEvent(SynapsePlayer player, bool allow, SynapseItem itemToDrop, bool @throw) : base(player, allow)
    {
        ItemToDrop = itemToDrop;
        Throw = @throw;
    }

    public SynapseItem ItemToDrop { get; set; }

    public bool Throw { get; set; }
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

public class OpenWarheadButtonEvent : PlayerInteractEvent
{
    public OpenWarheadButtonEvent(SynapsePlayer player, bool allow, bool openButton) : base(player, allow)
    {
        OpenButton = openButton;
    }

    public bool OpenButton { get; set; }
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

public class FallingIntoAbyssEvent : PlayerInteractEvent
{
    public FallingIntoAbyssEvent(SynapsePlayer player, bool allow) : base(player, allow) { }
}

public class SimpleSetClassEvent : PlayerEvent
{
    public RoleTypeId PreviousRole { get; }

    public RoleTypeId NextRole { get; }

    public SimpleSetClassEvent(SynapsePlayer player, RoleTypeId previousRole, RoleTypeId nextRole) : base(player)
    {
        PreviousRole = previousRole;
        NextRole = nextRole;
    }
}

public class UpdateDisplayNameEvent : PlayerEvent
{
    public UpdateDisplayNameEvent(SynapsePlayer player, string newDisplayName) : base(player)
    {
        NewDisplayName = newDisplayName;
    }

    public string NewDisplayName { get; set; }
}

public class CheckKeyCardPermissionEvent : PlayerInteractEvent
{
    public KeycardPermissions RequiredPermission { get; }

    public CheckKeyCardPermissionEvent(SynapsePlayer player, bool allow, KeycardPermissions requiredPermission) : base(player, allow)
    {
        RequiredPermission = requiredPermission;
    }
}

public class CallVanillaElevatorEvent : PlayerInteractEvent
{
    //TODO:
    /*
    public SynapseElevator Elevator { get; }

    public VanillaDestination RequestedDestination { get; }

    public CallVanillaElevatorEvent(SynapsePlayer player, bool allow, SynapseElevator elevator, VanillaDestination requestedDestination) : base(player, allow)
    {
        Elevator = elevator;
        RequestedDestination = requestedDestination;
    }
    */
    public CallVanillaElevatorEvent(SynapsePlayer player, bool allow) : base(player, allow)
    {
    }
}

public class SendPlayerDataEvent : PlayerEvent
{
    public SynapsePlayer PlayerToSee { get; set; }

    public bool IsInvisible { get; set; }

    public Vector3 Position { get; set; }

    public float Rotation { get; set; }

    public SendPlayerDataEvent(SynapsePlayer player) : base(player) { }
}

public class ChangeRoleEvent : PlayerEvent
{
    public uint RoleId { get; set; }

    public ChangeRoleEvent(SynapsePlayer player) : base(player) { }
}

public class KickEvent : PlayerInteractEvent
{
    public KickEvent(SynapsePlayer kickedPlayer, SynapsePlayer admin, string reason, bool allow) : base(
        kickedPlayer, allow)
    {
        Admin = admin;
        Reason = reason;
    }

    public SynapsePlayer Admin { get; }

    public string Reason { get; set; }
}

public class SpeakEvent : PlayerInteractEvent
{
    public SpeakEvent(SynapsePlayer player, bool allow, VoiceChatChannel channel) : base(player, allow)
    {
        Channel = channel;
    }
    
    public VoiceChatChannel Channel { get; set; }
}

public class SpeakToPlayerEvent : SpeakEvent
{
    public SynapsePlayer Receiver { get; }

    public SpeakToPlayerEvent(SynapsePlayer player, SynapsePlayer receiver, bool allow, VoiceChatChannel channel) :
        base(player, allow, channel)
        => Receiver = receiver;
}
