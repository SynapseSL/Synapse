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
using Scp914;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Map.Scp914;
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
            if (reactor.Key == typeof(SendPlayerDataEvent)) continue;
            if (reactor.Key.IsAbstract) continue;
            reactor.Value.SubscribeUnsafe(this, method);
        }
        _player.KeyPress.Subscribe(OnKeyPress);
        _round.Start.Subscribe(OnStart);
    }

    private void OnStart(RoundStartEvent args)
    {
        RegisterProcess();
    }

    public void Event(IEvent ev)
    {
        Logger.Warn("Event triggered: " + ev.GetType().Name);
    }

    SynapseDummy testDummy;
    private bool invis = false;
    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                testDummy = new SynapseDummy(ev.Player.Position, Quaternion.identity, RoleTypeId.ClassD, "Dummy");
                break;
           
            case KeyCode.Alpha2:
                invis = !invis;
                break;

            case KeyCode.Alpha3:
                ev.Player.SendFakeEffectIntensityFor(testDummy.Player, Effect.Invisible, 1);
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
                        scp939.Sound(testDummy.Position);
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

    private void RegisterProcess()
    {
        var scp914 = Synapse.Get<Scp914Service>();
        scp914.Synapse914Processors[(uint)ItemType.Medkit].Insert(0, new Process1());
        scp914.Synapse914Processors[(uint)ItemType.Medkit].Insert(0, new Process2());
    }

    class Process1 : ISynapse914Processor
    {
        public bool CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
        {
            item.Destroy();
            new SynapseItem(ItemType.ArmorHeavy, position);
            return true;
        }
    }

    class Process2 : ISynapse914Processor
    {
        public bool CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
        {
            if (UnityEngine.Random.Range(1, 5) > 2)
            {
                item.Destroy();
                new SynapseItem(ItemType.Flashlight, position);
                return true;
            }
            return false;
        }
    }
}
#endif
