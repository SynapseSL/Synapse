using System;
using InventorySystem.Items.MicroHID;
using Neuron.Core.Events;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using UnityEngine;
using Random = UnityEngine.Random;


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
            reactor.Value.SubscribeUnsafe(this, method);
        }
        
        _player.DoorInteract.Subscribe(OnDoor);
        _player.KeyPress.Subscribe(OnKeyPress);
        _round.SelectTeam.Subscribe(SelectTeam);
        _round.SpawnTeam.Subscribe(SpawnTeam);

        _item.KeyCardInteract.Subscribe(KeyCardItem);
        _item.BasicInteract.Subscribe(BasicItem);

        _map.Scp914Upgrade.Subscribe(ev =>
        {
            ev.MoveVector = Vector3.up * 5;
        });

        _item.Shoot.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Shoot {ev.Player.NickName} {ev.Target?.NickName} {ev.Item.ItemType}");
        });

        _scp.Scp049Revive.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp049Revive {ev.Scp.NickName} -> {ev.HumanToRevive.NickName}");
            
        });

        _item.ThrowGrenade.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Throw {ev.State}");
        });

        _item.MicroUse.Subscribe(ev =>
        {
            if (ev.MicroState == HidState.PoweringUp)
            {
                ev.AllowChangingState = false;
                ev.MicroState = HidState.Firing;
            }
        });

        _player.Death.Subscribe(ev =>
        {
            ev.RagDollInfo = "He's dead men";
            ev.DeathMessage = "Your dead";
        });

        _player.WalkOnHazard.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"HAZARD {ev.Player.NickName}");
        });
        
        _player.WarheadPanelInteract.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"WarheadPanelInteract {ev.Player.NickName} {ev.Operation}");
            ev.Operation = PlayerInteract.AlphaPanelOperations.Lever;
        });

        _player.Ban.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Ban {ev.Player.NickName} {ev.Duration}");
        });

        _map.GeneratorEngage.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"GeneratorEngage {ev.Generator.Name}");
            //ev.Deactivate();
        });

        _map.CancelWarhead.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"CancelWarhead");
        });

        _scp.Scp106Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp106Attack {ev.Scp.NickName}");
        });

        _scp.Scp173PlaceTantrum.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp173PlaceTantrum {ev.Scp.NickName}");
        });

        _scp.Scp173ActivateBreakneckSpeed.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp173ActivateBreakneckSpeed {ev.Scp.NickName}");
        });

        _scp.Scp049Revive.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp049Revive {ev.Scp.NickName}");
        });

        _scp.Scp0492Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp0492Attack {ev.Scp.NickName}");
        });

        _scp.Scp049Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp049Attack {ev.Scp.NickName}");
        });

        _scp.Scp096Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp096Attack {ev.Scp.NickName}");
        });

        _scp.Scp106Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp106Attack {ev.Scp.NickName}");
            ev.TakeToPocket = false;
            ev.Cooldown = 100f;
            ev.Damage = -1;
        });

        _scp.Scp173Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp173Attack {ev.Scp.NickName}");
            ev.Damage = 10;
        });

        _scp.Scp939Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp939Attack {ev.Scp.NickName}");
        });

        _item.FlipCoin.Subscribe(ev =>
        {
            ev.Tails = true;
        });

        _player.Pickup.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Pickup {ev.Player.NickName}, {ev.Item.Name}");
        });
        //Debug--

        _player.StartWorkStation.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"WorkStation {ev.Player.NickName}");
        });
        _player.Damage.Subscribe(ev =>
            NeuronLogger.For<Synapse>().Warn($"Damage: {ev.Player.NickName} {ev.Damage} {ev.DamageType}"));

        _player.FallingIntoAbyss.Subscribe(ev =>
                NeuronLogger.For<Synapse>().Warn($"{ev.Player.NickName} falled into an abyss"));

        _server.PreAuthentication.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Pre Auth {ev.UserId} " + ev.Country);
        });

        _scp.Scp049Attack.Subscribe(ScpEvent);
        _scp.Scp0492Attack.Subscribe(ScpEvent);
        _scp.Scp173Attack.Subscribe(ScpEvent);
        _scp.Scp096Attack.Subscribe(ScpEvent);
        _scp.Scp939Attack.Subscribe(ScpEvent);
        _scp.Scp106Attack.Subscribe(ScpEvent);

        _scp.Scp079Contain.Subscribe(ev => NeuronLogger.For<Synapse>().Warn("Contain 079: " + ev.Status));

        _scp.Scp079DoorInteract.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn("079 Door");
        });

        Synapse.Get<SynapseObjectEvents>().ButtonPressed
            .Subscribe(ev =>
            {
                NeuronLogger.For<Synapse>().Warn("Button Pressed " + ev.ButtonId);
                ev.Player.SendBroadcast("You pressed me!", 5);
            });

        _player.Escape.Subscribe(ev =>
        {
            if (ev.EscapeType == EscapeType.TooFarAway) return;
            Logger.Warn("ESCAPE " + ev.Player.NickName + " " + ev.EscapeType);
        });

        _player.SetClass.Subscribe(ev =>
        {
            if (ev.Role is RoleTypeId.Tutorial or RoleTypeId.Scientist)
            {
                ev.SpawnFlags = RoleSpawnFlags.None;
                ev.Player.Position = new Vector3(41f, 1014f, -33f);
                ev.Player.Inventory.ClearAllItems();
                ev.Player.Inventory.GiveItem(ItemType.Coin);
            }
        });
    }

    public void Event(IEvent ev)
    {
        Logger.Warn("Event triggered: " + ev.GetType().Name);
    }

    private void ScpEvent(ScpAttackEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn($"{ev.ScpAttackType} {ev.Damage} {ev.Scp?.NickName} | {ev.Victim?.NickName}");
    }
    
    private void OnDoor(DoorInteractEvent ev)
    {
        
    }

    private void KeyCardItem(KeyCardInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Keycard Use State: " + ev.State + " " + ev.Allow);
    }
    private void BasicItem(BasicItemInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Basic Item Use State: " + ev.State);
    }

    private void SelectTeam(SelectTeamEvent ev)
    {
        //ev.TeamId = 15;
        NeuronLogger.For<Synapse>().Warn("Team Selected " + ev.TeamId);
    }

    private void SpawnTeam(SpawnTeamEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("SpawnTeam: " + ev.TeamId);
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
                break;
           
            case KeyCode.Alpha2:
                testDummy.RotateToPosition(ev.Player.Position);
                testDummy.Movement = PlayerMovementState.Walking;
                testDummy.Direction = MovementDirection.Forward;
                break;

            case KeyCode.Alpha3:
                Logger.Warn("All Player that observes 173:");
                foreach (var observer in ev.Player.MainScpController.Scp173.Observer)
                {
                    Logger.Warn(observer.NickName);
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
