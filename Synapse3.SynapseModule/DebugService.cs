using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Teams;
using System;
using System.Linq;
using MapGeneration;
using MEC;
using Mirror;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Synapse3.SynapseModule.Map.Rooms;

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
            if (reactor.Key == typeof(UpdateEvent)) continue;
            if (reactor.Key == typeof(EscapeEvent)) continue;
            if (reactor.Key == typeof(Scp173ObserveEvent)) continue;
            if (reactor.Key == typeof(KeyPressEvent)) continue;
            if (reactor.Key == typeof(SpeakEvent)) continue;
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
        /*_round.Waiting.Subscribe(ev =>
        {
            Timing.CallDelayed(1f, () =>
            {
                try
                {
                    (Synapse.Get<RoomService>().GetRoom((uint)RoomType.TestingRoom) as SynapseNetworkRoom).Position +=
                        Vector3.up * 5;
                    (Synapse.Get<RoomService>().GetRoom((uint)RoomType.Scp330) as SynapseNetworkRoom).Position +=
                        Vector3.up * 5;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            });
        });*/
    }

    public void Event(IEvent ev)
    {
        Logger.Warn("Event triggered: " + ev.GetType().Name);
    }

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                SynapseLogger<DebugService>.Warn(ev.Player.Room.Doors.Count);
                break;
            case KeyCode.Alpha2:
                (ev.Player.Room as IVanillaRoom).RoomColor = Color.green;
                break;
            
            case KeyCode.Alpha3:
                (ev.Player.Room as IVanillaRoom).RoomColor = default;
                break;
        }
    }
}
#endif
