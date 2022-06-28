﻿using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Events;

public class PlayerEvents : Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<LoadComponentEvent> LoadComponent = new();

    public readonly EventReactor<KeyPressEvent> KeyPress = new();

    public PlayerEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(LoadComponent);
        _eventManager.RegisterEvent(KeyPress);
    }
}

public class LoadComponentEvent : IEvent
{
    public LoadComponentEvent(SynapsePlayer player)
    {
        Player = player;
        PlayerGameobject = player.gameObject;
    }
    
    public SynapsePlayer Player { get; }
    
    public GameObject PlayerGameobject { get; }

    public TComponent AddComponent<TComponent>() where TComponent : Component
    {
        var comp = (TComponent)PlayerGameobject.GetComponent(typeof(TComponent));
        if (comp == null)
            return PlayerGameobject.AddComponent<TComponent>();

        return comp;
    }
}

public class KeyPressEvent : IEvent
{
    public SynapsePlayer Player { get; }
    public KeyCode KeyCode { get; }

    public KeyPressEvent(SynapsePlayer player, KeyCode keyCode)
    {
        Player = player;
        KeyCode = keyCode;
    }
}