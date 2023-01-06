using System;
using Neuron.Core.Events;
using System.Data;
using System.Reflection;
using InventorySystem.Items.MicroHID;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using Respawning;
using Respawning.NamingRules;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
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
        Synapse.Get<SynapseCommandService>().ServerConsole.Subscribe(ev => Logger.Warn(ev.Context.FullCommand));

        var method = ((Action<IEvent>)Event).Method;
        foreach (var reactor in _event.Reactors)
        {
            if (reactor.Key == typeof(UpdateObjectEvent)) continue;
            if (reactor.Key == typeof(EscapeEvent)) continue;
            if (reactor.Key == typeof(Scp173ObserveEvent)) continue;
            if (reactor.Key == typeof(KeyPressEvent)) continue;
            if (reactor.Key == typeof(RoundCheckEndEvent)) continue;
            if (reactor.Key.IsAbstract) continue;
            reactor.Value.SubscribeUnsafe(this, method);
        }
        _player.KeyPress.Subscribe(OnKeyPress);
    }

    public void Event(IEvent ev)
    {
        Logger.Warn("Event triggered: " + ev.GetType().Name);
    }

    SynapseDummy testDummy;

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                ev.Player.ActiveHint.Clear();
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(0, "I like trians", 15, HintSide.Left));
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(10, "Yea Trains", 10, HintSide.Left));
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(1, "LOOK ME !", 7, HintSide.Right));
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(20, "I AME A long string to long to be display in one part! so the other parte is under me, real look that is amazing !", 17, HintSide.Right));

                break;

            case KeyCode.Alpha4:
                ev.Player.ActiveHint.Clear();
                space += 0.01f;
                ev.Player.ActiveHint.AddWithoutUpdate(new SynapseTextHint(20, "OOOO<<>>", 5, HintSide.Left));
                ev.Player.ActiveHint.Add(new SynapseTextHint(21, $"<mspace={space}em><size={size}%>O|</size></mspace>{space} {size}", 5, HintSide.Left));
                break;
        }
    }
}
#endif
