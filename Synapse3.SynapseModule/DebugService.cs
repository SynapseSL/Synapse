using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MEC;
using Mirror;
using Neuron.Core;
using Neuron.Core.Events;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Object = UnityEngine.Object;


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

        _round.Waiting.Subscribe(ev =>
            Synapse.Get<SchematicService>().SpawnSchematic(SynapseTestSchematic(), new Vector3(26f, 992f, -41)));
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
                Synapse.Get<ReferenceHub>();
                break;
           
            case KeyCode.Alpha2:
                break;

            case KeyCode.Alpha3:
                //CheckObjects(ev.Player).RunSafelyCoroutine();
                //var door = new SynapseDoor(SynapseDoor.SpawnableDoorType.Hcz, ev.Player.Position + ev.Player.transform.forward * 3, Quaternion.identity,
                //    Vector3.one);
                //Timing.CallDelayed(3f, () => door.Position = ev.Player.Position + ev.Player.transform.forward * 3);
                var schematic = Synapse.Get<SchematicService>().SpawnSchematic(SynapseTestSchematic(), ev.Player.Position);
                Timing.CallDelayed(3f, () => schematic.Position = ev.Player.Position);
                break;

            case KeyCode.Alpha4:
                switch (ev.Player.RoleType)
                {
                    case RoleTypeId.Scp173:
                        var scp173 = ev.Player.MainScpController.Scp173;
                        scp173.BlinkCooldownPerPlayer = 5;
                        scp173.BlinkCooldownBase = 10;
                        NeuronLogger.For<Synapse>().Warn("Observer: " + scp173.Observer.Count);
                        break;
                    case RoleTypeId.Scp106:
                        var scp106 = ev.Player.MainScpController.Scp106;
                        NeuronLogger.For<Synapse>().Warn("PoketPlayer: " + scp106.PlayersInPocket.Count);
                        break;
                    case RoleTypeId.Scp079:
                        var scp079 = ev.Player.MainScpController.Scp079;
                        scp079.RegenEnergy = 200;
                        scp079.Exp = 3;
                        break;
                    case RoleTypeId.Scp096:
                        var scp096 = ev.Player.MainScpController.Scp096;
                        scp096.CurrentShield = 10;
                        scp096.MaxShield = 100;
                        scp096.ShieldRegeneration = 2000;
                        break;
                    case RoleTypeId.Scp939:
                        var scp939 = ev.Player.MainScpController.Scp939;
                        scp939.Sound(testDummy.Position, 2);//TODO
                        scp939.AmnesticCloudCooldown = 4;
                        scp939.MimicryCloudCooldown = 4;
                        NeuronLogger.For<Synapse>().Warn("MinicryPointPositioned: " + scp939.MinicryPointPositioned);
                        break;
                }
                break;
            
            case KeyCode.Alpha5:
                for (int i = 0; i < NetworkClient.prefabs.Count; i++)
                {
                    if (i == 0) continue;
                    var prefab = NetworkClient.prefabs.ElementAt(i);
                    Timing.CallDelayed(i * 0.5f,
                        () => NetworkServer.Spawn(Object.Instantiate(prefab.Value, ev.Player.Position,
                            Quaternion.identity)));
                }
                break;
        }
    }

    private SchematicConfiguration SynapseTestSchematic()
    {
        return new SchematicConfiguration()
        {
            Name = "test",
            Id = 99999,
            Doors = new List<SchematicConfiguration.DoorConfiguration>()
            {
                new SchematicConfiguration.DoorConfiguration()
                {
                    Position = Vector3.left * 6,
                    Health = 100,
                    Locked = false,
                    DoorType = SynapseDoor.SpawnableDoorType.Ez,
                    UnDestroyable = false,
                }
            },
            Generators = new List<SchematicConfiguration.SimpleUpdateConfig>()
            {
                new SchematicConfiguration.SimpleUpdateConfig()
                {
                    Position = Vector3.forward * 3,
                }
            },
            /*
            Dummies = new List<SchematicConfiguration.DummyConfiguration>()
            {
                new SchematicConfiguration.DummyConfiguration()
                {
                    Position = Vector3.forward *-3,
                    Name = "Test Dummy",
                    Role = RoleTypeId.ClassD,
                    Badge = "",
                    BadgeColor = ""
                }
            },
            */
            Lights = new List<SchematicConfiguration.LightSourceConfiguration>()
            {
                new SchematicConfiguration.LightSourceConfiguration()
                {
                    Position = Vector3.up *3,
                    Color = Color.white,
                    LightIntensity = 1,
                    LightRange = 10,
                    LightShadows = true
                }
            },
            Lockers = new List<SchematicConfiguration.LockerConfiguration>()
            {
                new SchematicConfiguration.LockerConfiguration()
                {
                    Position = Vector3.left * 3,
                    LockerType = SynapseLocker.LockerType.ScpPedestal,
                    DeleteDefaultItems = false,
                }
            },
            Primitives = new List<SchematicConfiguration.PrimitiveConfiguration>()
            {
                new SchematicConfiguration.PrimitiveConfiguration()
                {
                    Position = Vector3.right * 3,
                    Color = Color.white,
                    Physics = false,
                }
            },
            Ragdolls = new List<SchematicConfiguration.RagdollConfiguration>()
            {
                new SchematicConfiguration.RagdollConfiguration()
                {
                    Position = Vector3.forward,
                    DamageType = DamageType.Bleeding,
                    Nick = "Test RagDoll",
                    RoleType = RoleTypeId.ClassD,
                }
            },
            Items = new List<SchematicConfiguration.ItemConfiguration>()
            {
                new SchematicConfiguration.ItemConfiguration()
                {
                    Position = Vector3.forward * -1,
                        Physics = false,
                        ItemType = ItemType.MicroHID,
                        CanBePickedUp = false,
                        Attachments = 0,
                        Durabillity = 0,
                }
            },
            Targets = new List<SchematicConfiguration.TargetConfiguration>()
            {
                new SchematicConfiguration.TargetConfiguration()
                {
                    Position = Vector3.forward * 6,
                    TargetType = SynapseTarget.TargetType.DBoy,
                }
            },
            WorkStations = new List<SchematicConfiguration.SimpleUpdateConfig>()
            {
                new SchematicConfiguration.SimpleUpdateConfig()
                {
                    Position = Vector3.forward * -6,
                }
            }
        };
    }

    private IEnumerator<float> CheckObjects(SynapsePlayer player)
    {
        var primitive = new SynapsePrimitive(PrimitiveType.Cube, Color.white, player.Position, Quaternion.identity,
            Vector3.one);
        yield return Timing.WaitForSeconds(3f);
        primitive.Position = player.Position;

        yield return Timing.WaitForSeconds(3f);
        var light = new SynapseLight(Color.white, 1f, 10f, true, player.Position, Quaternion.identity, Vector3.one);
        yield return Timing.WaitForSeconds(3f);
        light.Position = player.Position;
        
        yield return Timing.WaitForSeconds(3f);
        var target = new SynapseTarget(SynapseTarget.TargetType.DBoy, player.Position, Quaternion.identity,
            Vector3.one);
        yield return Timing.WaitForSeconds(3f);
        target.Position = player.Position;
        
        yield return Timing.WaitForSeconds(3f);
        //Doors can't spawn inside a player or the AC escalates
        var door = new SynapseDoor(SynapseDoor.SpawnableDoorType.Hcz, player.Position + player.transform.forward * 3, Quaternion.identity,
            Vector3.one);
        yield return Timing.WaitForSeconds(3f);
        door.Position = player.Position + player.transform.forward * 3;
        
        yield return Timing.WaitForSeconds(3f);
        var workstation = new SynapseWorkStation(player.Position, Quaternion.identity, Vector3.one);
        yield return Timing.WaitForSeconds(3f);
        if (workstation != null)
            workstation.Position = player.Position;
        
        yield return Timing.WaitForSeconds(3f);
        var locker = new SynapseLocker(SynapseLocker.LockerType.ScpPedestal, player.Position, Quaternion.identity,
            Vector3.one);
        yield return Timing.WaitForSeconds(3f);
        locker.Position = player.Position;
        
        yield return Timing.WaitForSeconds(3f);
        var gen = new SynapseGenerator(player.Position, Quaternion.identity, Vector3.one);
        yield return Timing.WaitForSeconds(3f);
        gen.Position = player.Position;
        
        yield return Timing.WaitForSeconds(3f);
        var item = new SynapseItem(ItemType.MicroHID, player.Position);
        yield return Timing.WaitForSeconds(3f);
        item.Position = player.Position;
        
        yield return Timing.WaitForSeconds(3f);
        var rag = new SynapseRagDoll(RoleTypeId.ClassD, player.Position, Quaternion.identity, Vector3.one, null,
                DamageType.Asphyxiated, "");
        yield return Timing.WaitForSeconds(3f);
        if (rag != null)
            rag.Position = player.Position;

        yield return Timing.WaitForSeconds(3f);
        var dummy = new SynapseDummy(player.Position, Quaternion.identity, RoleTypeId.ClassD, "");
        yield return Timing.WaitForSeconds(3f);
        dummy.Position = player.Position;
    }

    private void StoreNetworkObjects()
    {
        var msg = "Manager Prefabs:\n";

        foreach (var spawnPrefab in NetworkManager.singleton.spawnPrefabs)
        {
            msg += spawnPrefab.name + "\n";
        }

        msg += "\nClient Prefabs:\n";

        foreach (var prefab in NetworkClient.prefabs)
        {
            msg += prefab.Value.name + "    -   " + prefab.Key + "\n";
        }

        File.WriteAllText(Synapse.Get<NeuronBase>().RelativePath("Prefabs.txt"), msg);
    }
}
#endif
