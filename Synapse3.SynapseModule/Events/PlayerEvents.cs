using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public class PlayerEvents : Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<LoadComponentEvent> LoadComponent = new();
    public readonly EventReactor<KeyPressEvent> KeyPress = new();
    public readonly EventReactor<HarmPermissionEvent> HarmPermission = new();

    public PlayerEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(LoadComponent);
        _eventManager.RegisterEvent(KeyPress);
        _eventManager.RegisterEvent(HarmPermission);
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
    public LoadComponentEvent(SynapsePlayer player) : base(player)
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