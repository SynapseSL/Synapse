using System.Collections.Generic;
using System.IO;
using System.Linq;
using MEC;
using Mirror;
using Neuron.Core;
using Neuron.Core.Events;
using Neuron.Core.Logging;
using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Events;
using System;
using System.Collections.Generic;
using InventorySystem.Items.MicroHID;
using MEC;
using PlayerRoles;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Role;
using UnityEngine;


namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;
    private MapEvents _map;
    private RoundEvents _round;
    private ItemEvents _item;
    private ScpEvents _scp;
    private ServerEvents _server;
    private EventManager _event;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, ScpEvents scp,
        ServerEvents server, EventManager eventManager)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _server = server;
        _scp = scp;
        _event = eventManager;
    }

    public override void Enable()
    {
        var method = ((Action<IEvent>)Event).Method;
        foreach (var reactor in _event.Reactors)
        {
            if (reactor.Key == typeof(UpdateObjectEvent)) continue;
            if (reactor.Key == typeof(UpdateEvent)) continue;
            if (reactor.Key == typeof(EscapeEvent)) continue;
            if (reactor.Key == typeof(Scp173ObserveEvent)) continue;
            if (reactor.Key == typeof(KeyPressEvent)) continue;
            if (reactor.Key == typeof(SpeakEvent)) continue;
            if (reactor.Key == typeof(SpeakToPlayerEvent)) continue;
            if (reactor.Key == typeof(RoundCheckEndEvent)) continue;
            if (reactor.Key == typeof(SendPlayerDataEvent)) continue;
            if (reactor.Key.IsAbstract) continue;
            reactor.Value.SubscribeUnsafe(this, method);
        }
        _player.KeyPress.Subscribe(OnKeyPress);
        _item.ConsumeItem.Subscribe(ev =>
        {
            if (ev.State == ItemInteractState.Finalize)
                ev.Allow = false;
        });
        _player.Escape.Subscribe(ev =>
        {
            if(ev.EscapeType == EscapeType.NotAssigned)
                Logger.Warn("Escape not assigned");
        });
    }
    
    public void Event(IEvent e)
    {
        switch (e)
        {
            default:
                Logger.Warn("Event triggered: " + e.GetType().Name);
                break;
        }
    }

    private SynapseDummy _dummy;

    private void OnKeyPress(KeyPressEvent ev)
    {

        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(0, "I like trains, \\<Wagon\\>", 15, HintSide.Left));
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(10, "Yes I like trains", 10, HintSide.Left));
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(33, "Text here...", 10, HintSide.Midle));
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(20, "<color=red>Can you see me?</color> Yes you can!", 7, HintSide.Right));
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(20, "I ame on the other sied that is amazing", 7, HintSide.Left));
                ev.Player.ActiveHint.UpdateText();
                break;
            case KeyCode.Alpha2:
                ev.Player.ActiveHint.Add(new SynapseTextHint(20, "<b><color=blue>I ame a string of size 2, <color=red>and to long to be display</color></b> in one part!", 17, HintSide.Right, 2, 200));
                break;
            case KeyCode.Alpha3:
                ev.Player.ActiveHint.Clear();
                break;
        }
    }
}
#endif