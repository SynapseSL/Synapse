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
using MapGeneration.Distributors;
using PluginAPI.Core.Zones.Heavy;
using Synapse3.SynapseModule.Map;
using System.Diagnostics.Eventing.Reader;
using PluginAPI.Core;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Config;

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
                Synapse.Get<ElevatorService>().Elevators.FirstOrDefault(x => x.ElevatorId == 99).MoveToNext();
                break;
           
            case KeyCode.Alpha2:
                var pos = Vector3.zero;
                var points = WaypointBase.AllWaypoints.Where(x => x != null).Reverse().Take(5);
                foreach (var waypoint in points)
                {
                    switch (waypoint)
                    {
                        case NetIdWaypoint netIdWaypoint:
                            pos = netIdWaypoint.transform.position;
                            break;
                            
                        case ElevatorWaypoint elevatorWaypoint:
                            pos = elevatorWaypoint.ElevatorTransform.position;
                            break;
                            
                        default:
                            Logger.Warn("Other Waypoint?");
                            break;
                    }
                
                    Logger.Warn(Synapse.Get<RoomService>().Rooms
                                    .OrderBy(x => Vector3.Distance(ev.Player.Position, x.Position)).First().Name +
                                " " +
                                pos);    
                }
                
                break;
            case KeyCode.Alpha3:
                new SynapseRagDoll(RoleTypeId.ClassD, ev.Player.Position, Quaternion.identity, Vector3.one, ev.Player,
                    DamageType.Unknown, "Elevator Dummy")
                {
                    MoveInElevator = true
                };
                break;

            case KeyCode.Alpha4:
                var dummy = SpawnDebugRole(ev.Player);

            dummy.Player.RoleType = RoleTypeId.ClassD;
            dummy.Player.FakeRoleManager.OwnVisibleRole = RoleTypeId.Scientist;
            break;
/*
            case KeyCode.Alpha4:
                ev.Player.SendFakeEffectIntensity(Effect.Hypothermia, 1);

                break;*/

                /*                case KeyCode.Alpha5:
                                    var dummy2 = SpawnDebugRole(ev.Player);
                                    Timing.CallDelayed(1f, () =>
                                    {
                                        dummy2.Player.FakeRoleManager.UpdateAll();
                                        dummy2.Player.FakeRoleManager.VisibleRole = RoleTypeId.ClassD;
                                        dummy2.Player.FakeRoleManager.UpdateAll();
                                    });
                                break;*/

                /* case KeyCode.Alpha4:
                     SpawnDebugShematic(ev.Player);
                     break;


                 case KeyCode.Alpha5:
                     //The generator start to flick
                     Schematic.HideFromAll();
                     break;
                 case KeyCode.Alpha6:
                     Schematic.ShowAll();
                     break;
                 case KeyCode.Alpha7:
                     Schematic.HideFromPlayer(ev.Player);
                     break;
                 case KeyCode.Alpha8:
                     Schematic.ShowPlayer(ev.Player);
                     break;
                 case KeyCode.Alpha9:
                     Schematic.Position = ev.Player.Position;
                     break;*/

        }
    }

    class CustomRoleTest : SynapseAbstractRole
    {
        class config : IAbstractRoleConfig
        {
            public RoleTypeId Role => RoleTypeId.NtfCaptain;

            public RoleTypeId VisibleRole => RoleTypeId.None;//Unsyc

            public RoleTypeId OwnRole => RoleTypeId.None;

            public uint EscapeRole => (uint)RoleTypeId.ClassD;

            public float Health => 100;

            public float MaxHealth => 120;

            public float ArtificialHealth => 0;

            public float MaxArtificialHealth => 200;

            public RoomPoint[] PossibleSpawns => null;

            public SerializedPlayerInventory[] PossibleInventories => null;

            public bool CustomDisplay => true;

            public bool Hierarchy => true;//Hide all custom info

            public bool UseCustomUnitName => true;//Not present

            public string CustomUnitName => "Hi my unite name";

            public SerializedVector3 Scale => Vector3.one;
        }

        protected override bool CanSeeUnit(SynapsePlayer player) => true;

        protected override bool HigherRank(SynapsePlayer player) => true;
        protected override bool LowerRank(SynapsePlayer player) => false;
        protected override bool SameRank(SynapsePlayer player) => false;

        protected override IAbstractRoleConfig GetConfig() => new config();
    }

    private SynapseDummy SpawnDebugRole(SynapsePlayer player)
    {
        var roleService = Synapse.Get<RoleService>();
        if (!roleService.IsIdRegistered(999))
        {
            roleService.RegisterRole(new RoleAttribute()
            {
                Id = 999,
                Name = "Debug Role",
                RoleScript = typeof(CustomRoleTest),
                TeamId = (uint)Team.FoundationForces
            });
        }
        var dummy = new SynapseDummy(player.Position, player.Rotation, RoleTypeId.ClassD, "Hello", "I can destroy your server at any time!");
        dummy.Player.RoleID = 999;
        return dummy;
    }

    private void SpawnDebugShematic(SynapsePlayer player)
    {
        var schematicService = Synapse.Get<SchematicService>();
        var configuration = new SchematicConfiguration()
        {
            Lockers = new (),
            Primitives = new (),
            Doors = new (),
            Generators = new (),
            Lights = new (),
            WorkStations = new (),
            Targets = new()
        };

        var position = Vector3.zero;

        foreach (var lockerType in (SynapseLocker.LockerType[])Enum.GetValues(typeof(SynapseLocker.LockerType)))
        {
            configuration.Lockers.Add(new SchematicConfiguration.LockerConfiguration()
            {
                DeleteDefaultItems = false,
                LockerType = lockerType,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
                Update = false,
                UpdateFrequency = -1
            });
            position += Vector3.forward;
        }

        foreach (var primitiveType in (PrimitiveType[])Enum.GetValues(typeof(PrimitiveType)))
        {
            configuration.Primitives.Add(new SchematicConfiguration.PrimitiveConfiguration()
            {
                Physics = false,
                Color = Color.white,
                PrimitiveType = primitiveType,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
            });
            position += Vector3.forward;
        }

        foreach (var doorType in (SynapseDoor.SpawnableDoorType[])Enum.GetValues(typeof(SynapseDoor.SpawnableDoorType)))
        {
            if (doorType == SynapseDoor.SpawnableDoorType.None) continue;

            configuration.Doors.Add(new SchematicConfiguration.DoorConfiguration()
            {
                DoorType = doorType,
                Health = 100,
                Locked = false,
                Open = false,
                UnDestroyable = false,
                Update = false,
                UpdateFrequency = -1,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
            });
            position += Vector3.forward;
        }

        configuration.Generators.Add(new SchematicConfiguration.SimpleUpdateConfig()
        {
            Update = true,
            UpdateFrequency = 2,
            CustomAttributes = new List<string>(),
            Position = position,
            Rotation = new Config.SerializedVector3(0, 0, 0),
            Scale = Vector3.one,
        });
        position += Vector3.forward;

        configuration.Lights.Add(new SchematicConfiguration.LightSourceConfiguration()
        {
            Color = Color.white,
            LightIntensity = 20,
            LightRange = 100,
            LightShadows = false,
            CustomAttributes = new List<string>(),
            Position = position,
            Rotation = new Config.SerializedVector3(0, 0, 0),
            Scale = Vector3.one,
        });
        position += Vector3.forward;

        configuration.WorkStations.Add(new SchematicConfiguration.SimpleUpdateConfig()
        {
            Update = true,
            UpdateFrequency = -1,
            CustomAttributes = new List<string>(),
            Position = position,
            Rotation = new Config.SerializedVector3(0, 0, 0),
            Scale = Vector3.one,
        });
        position += Vector3.forward;

        foreach (var targetType in (SynapseTarget.TargetType[])Enum.GetValues(typeof(SynapseTarget.TargetType)))
        {
            configuration.Targets.Add(new SchematicConfiguration.TargetConfiguration()
            {
                TargetType = targetType,
                CustomAttributes = new List<string>(),
                Position = position,
                Rotation = new Config.SerializedVector3(0, 0, 0),
                Scale = Vector3.one,
            });
            position += Vector3.forward;
        }

        Schematic = schematicService.SpawnSchematic(configuration, player.Position, player.Rotation);
    }

    private SynapseSchematic Schematic;

    private SynapseDummy Dummy;
}
#endif
