using Neuron.Core.Events;
using Neuron.Core.Meta;
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
    }
}

public abstract class PlayerEvent<TEvent> : SynapseEvent<TEvent> where TEvent : SynapseEvent<TEvent>
{
    public SynapsePlayer Player { get; }

    protected PlayerEvent(SynapsePlayer player, EventReactor<TEvent> reactor) : base(reactor)
    {
        Player = player;
    }
}

public abstract class PlayerInteractEvent<TEvent> : PlayerEvent<TEvent> where TEvent : SynapseEvent<TEvent>
{
    public bool Allow { get; set; }

    protected PlayerInteractEvent(SynapsePlayer player, bool allow, EventReactor<TEvent> reactor) : base(player,
        reactor)

    {
        Allow = allow;
    }
} 

public class LoadComponentEvent : PlayerEvent<LoadComponentEvent>
{
    public LoadComponentEvent(SynapsePlayer player) : base(player,Synapse.Get<PlayerEvents>().LoadComponent)
    {
        PlayerGameObject = player.gameObject;
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

public class KeyPressEvent : PlayerEvent<KeyPressEvent>
{
    public KeyCode KeyCode { get; }

    public KeyPressEvent(SynapsePlayer player, KeyCode keyCode) : base(player, Synapse.Get<PlayerEvents>().KeyPress)
    {
        KeyCode = keyCode;
    }
}

public class HarmPermissionEvent : SynapseEvent<HarmPermissionEvent>
{
    public HarmPermissionEvent(SynapsePlayer attacker, SynapsePlayer victim, bool allow) : base(
        Synapse.Get<PlayerEvents>().HarmPermission)
    {
        Victim = victim;
        Attacker = attacker;
        Allow = allow;
    }
    
    public bool Allow { get; set; }
    
    public SynapsePlayer Attacker { get; }
    
    public SynapsePlayer Victim { get; }
}

public class ShootEvent : PlayerInteractEvent<ShootEvent>
{
    public ShootEvent(SynapsePlayer player, uint targetNetID, ushort itemSerial, bool allow) :
        base(player, allow, Synapse.Get<PlayerEvents>().Shoot)
    {
        if (targetNetID > 0)
            Target = Synapse.Get<PlayerService>().GetPlayer(targetNetID);
        
        Item = Synapse.Get<ItemService>().GetSynapseItem(itemSerial);
    }
    
    public SynapsePlayer Target { get; }
    
    public SynapseItem Item { get; }
}