using System.Collections.Generic;
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
    public readonly EventReactor<ShootEvent> Shoot = new();
    public readonly EventReactor<SetClassEvent> SetClass = new();
    public readonly EventReactor<UpdateEvent> Update = new();
    public readonly EventReactor<DoorInteractEvent> DoorInteract = new();
    public readonly EventReactor<LockerUseEvent> LockerUse = new();
    public readonly EventReactor<StartWarheadEvent> StartWarhead = new();
    public readonly EventReactor<CancelWarheadEvent> CancelWarhead = new();
    public readonly EventReactor<WarheadPanelInteractEvent> WarheadPanelInteract = new();
    public readonly EventReactor<BanEvent> Ban = new();
    public readonly EventReactor<ChangeItemEvent> ChangeItem = new();
    public readonly EventReactor<DamageEvent> Damage = new();
    public readonly EventReactor<DeathEvent> Death = new();

    public PlayerEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(LoadComponent);
        _eventManager.RegisterEvent(KeyPress);
        _eventManager.RegisterEvent(HarmPermission);
        _eventManager.RegisterEvent(Shoot);
        _eventManager.RegisterEvent(SetClass);
        _eventManager.RegisterEvent(StartWarhead);
        _eventManager.RegisterEvent(CancelWarhead);
        _eventManager.RegisterEvent(DoorInteract);
        _eventManager.RegisterEvent(LockerUse);
        _eventManager.RegisterEvent(WarheadPanelInteract);
        _eventManager.RegisterEvent(Ban);
        _eventManager.RegisterEvent(ChangeItem);
        _eventManager.RegisterEvent(Death);
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

public class ShootEvent : PlayerInteractEvent
{
    public ShootEvent(SynapsePlayer player, uint targetNetID, ushort itemSerial, bool allow) : base(player, allow)
    {
        if (targetNetID > 0)
            Target = Synapse.Get<PlayerService>().GetPlayer(targetNetID);
        
        Item = Synapse.Get<ItemService>().GetSynapseItem(itemSerial);
    }
    
    public SynapsePlayer Target { get; }
    
    public SynapseItem Item { get; }
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

    public List<int> Items { get; set; } = new();

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
    public LockerUseEvent(SynapsePlayer player, bool allow, SynapseLocker locker, SynapseLocker.SynapseLockerChamber chamber) : base(player, allow)
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

public class CancelWarheadEvent : PlayerInteractEvent
{
    public CancelWarheadEvent(SynapsePlayer player, bool allow) : base(player, allow) { }
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
    public DamageEvent(SynapsePlayer player, bool allow, SynapsePlayer attacker, DamageType damageType, float damage) : base(player, allow)
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