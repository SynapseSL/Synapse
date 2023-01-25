using System;
using Neuron.Core.Events;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
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
                testDummy?.Destroy();
                testDummy = new SynapseDummy(ev.Player.Position, ev.Player.Rotation, RoleTypeId.ClassD, "Test");
                testDummy.RaVisible = true;
                testDummy.DestroyWhenDied = false;
                break;
           
            case KeyCode.Alpha2:
                testDummy.RotateToPosition(ev.Player.Position);
                testDummy.Movement = PlayerMovementState.Walking;
                testDummy.Direction = MovementDirection.Forward;
                break;

            case KeyCode.Alpha3:
                foreach (var rag in Synapse.Get<MapService>()._synapseRagdolls)
                {
                    rag.SendFakeInfoToPlayer(ev.Player,
                        new RagdollData(rag.Owner.Hub, rag.Damage, RoleTypeId.ClassD, rag.Position, rag.Rotation,
                            rag.NickName, rag.CreationTime));
                }
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
        }
    }
}
#endif
