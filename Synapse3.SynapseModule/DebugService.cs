using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using Neuron.Core;
using PlayerRoles;
using RelativePositioning;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
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
        Synapse.Get<SchematicService>().RegisterSchematic(new SchematicConfiguration()
        {
            Name = "ElevatorChamber",
            Id = 50,
            Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>()
            {
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    Position = Vector3.down * 0.4f,
                    PrimitiveType = PrimitiveType.Plane,
                    Color = Color.white,
                    Scale = Vector3.one * 0.3f
                }
            }
        });
        Synapse.Get<SchematicService>().RegisterSchematic(new SchematicConfiguration()
        {
            Name = "ElevatorDestination",
            Id = 51,
            Doors = new List<SchematicConfiguration.DoorConfiguration>()
            {
                new SchematicConfiguration.DoorConfiguration()
                {
                    DoorType = SynapseDoor.SpawnableDoorType.Ez,
                    Position = Vector3.forward
                }
            }
        });
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
        _item.ConsumeItem.Subscribe(ev =>
        {
            if (ev.State == ItemInteractState.Finalize)
                ev.Allow = false;
        });
        _round.Waiting.Subscribe(ev =>
        {
            ((SynapseNetworkRoom)Synapse.Get<RoomService>().GetRoom((uint)RoomType.TestingRoom)).Position +=
                Vector3.up * 5;
            ((SynapseNetworkRoom)Synapse.Get<RoomService>().GetRoom((uint)RoomType.Scp330)).Position +=
                Vector3.up * 5;
            
            var text = "";
            foreach (var prefab in NetworkClient.prefabs)
            {
                text += "\n" + prefab.Value.name + " ID: " + prefab.Key;
            }

            File.WriteAllText(Path.Combine(Synapse.Get<NeuronBase>().RelativePath(), "prefabs.txt"), text);
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
    
    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                var dummy = new SynapseDummy(ev.Player.Position, ev.Player.Rotation, ev.Player.RoleType, "Dummy");
                dummy.RaVisible = true;
                dummy.Player.CustomInfo.Add("TestMessage Only for you", new List<SynapsePlayer>() { ev.Player });
                break;
        }
    }

    private SynapseDummy Dummy;
}
#endif
