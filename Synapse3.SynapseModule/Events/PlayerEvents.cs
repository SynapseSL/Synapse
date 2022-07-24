using System.Collections.Generic;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Item;
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