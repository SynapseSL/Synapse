using System;
using System.Reflection;
using InventorySystem.Items.MicroHID;
using MEC;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Permissions;
using Object = UnityEngine.Object;
using Synapse3.SynapseModule.Map.Rooms;
using PlayerRoles.PlayableScps.Scp106;
using System.Reflection;


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
    private SynapseCommandService _commandService;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, ScpEvents scp,
        SynapseCommandService commandService, ServerEvents server)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _server = server;
        _commandService = commandService;
        _scp = scp;
    }

    public override void Enable()
    {
        Synapse.Get<SynapseCommandService>().ServerConsole.Subscribe(ev => Logger.Warn(ev.Context.FullCommand));
        
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
            NeuronLogger.For<Synapse>().Warn($"{ev.Player.NickName} {ev.DamageType} {ev.LastTakenDamage} Message: {ev.DeathMessage ?? "NONE"} RagdollInfo: {ev.RagdollInfo ?? "NONE"}");
        });
        
        _player.WalkOnHazard.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"HAZARD {ev.Player.NickName}");
        });

        //--Debug
        _player.WarheadPanelInteract.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"WarheadPanelInteract {ev.Player.NickName}");
        });

        _player.DropItem.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"DropItem {ev.Player.NickName}");
        });

        _player.Heal.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Heal {ev.Player.NickName}");
        });

        _player.Join.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Join {ev.NickName}");
        });

        _player.PlaceBulletHole.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"PlaceBulletHole {ev.Player.NickName}");
        });

        _player.OpenWarheadButton.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"OpenWarheadButton {ev.Player.NickName}");
        });

        _player.UpdateDisplayName.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"UpdateDisplayName {ev.Player.NickName}");
        });

        _player.Ban.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Ban {ev.Player.NickName} {ev.Duration}");
        });

        _map.GeneratorEngage.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"GeneratorEngage {ev.Generator.Name}");
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
        });

        _scp.Scp173Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp173Attack {ev.Scp.NickName}");
        });

        _scp.Scp939Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp939Attack {ev.Scp.NickName}");
        });

        _item.FlipCoin.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp939Attack {ev.Player.NickName}");
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

        _scp.Scp049Revive.Subscribe(ev => ev.Allow = false);

        Synapse.Get<SynapseObjectEvents>().ButtonPressed
            .Subscribe(ev =>
            {
                NeuronLogger.For<Synapse>().Warn("Button Pressed " + ev.ButtonId);
                ev.Player.SendBroadcast("You pressed me!", 5);
            });

        _player.Kick.Subscribe(ev => Logger.Warn("KICK " + ev.Admin + " " + ev.Reason));
        _player.Ban.Subscribe(ev => Logger.Warn("Ban " + ev.Admin + " " + ev.Reason));
        
        _round.Decontamination.Subscribe(ev =>
        {
            Logger.Warn("Decontamination ");
            ev.Allow = false;
        });
        
        _player.Escape.Subscribe(ev =>
        {
            if(ev.EscapeType == EscapeType.TooFarAway) return;
            Logger.Warn("ESCAPE " + ev.Player.NickName + " " + ev.EscapeType);
        });
        
        _round.FirstSpawn.Subscribe(ev =>
        {
            Logger.Warn("First Spawn,SCPS: "+ ev.AmountOfScpSpawns);
        });

        _round.Start.Subscribe(ev =>//Log NW Event
        {
            Logger.Warn("Round Start");

            foreach (var @event in PluginAPI.Events.EventManager.Events)
            {
                Logger.Warn($"{@event.Key}");
                foreach (var method in @event.Value.Invokers)
                {
                    Logger.Warn($"{method.Key.FullName} {method.Value.Count}");

                }
            }

        
        _player.SetClass.Subscribe(ev =>
        {
            ev.Position = new Vector3(41f, 1014f,-33f);
            ev.HorizontalRotation = 270f;

        });
    }

    private void ScpEvent(ScpAttackEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn($"{ev.ScpAttackType} {ev.Damage} {ev.Scp.NickName} | {ev.Victim.NickName}");
        //ev.Allow = false;
    }
    
    private void OnDoor(DoorInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Door Interact");
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
    SynapseSchematic Schematic;
    private SerializedPlayerState _state;
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
                break;

            case KeyCode.Alpha3:
                Synapse.Get<SchematicService>().SpawnSchematic(2000, ev.Player.Position);
                break;

            case KeyCode.Alpha4:
                switch (ev.Player.RoleType)
                {
                    case RoleTypeId.Scp173:
                        var scp173 = ev.Player.ScpController.Scp173;
                        scp173.CurentBlinkCooldown = 5;//TODO
                        scp173.TantrumCoolDown = 2;//TODO
                        break;
                    case RoleTypeId.Scp106:
                        var scp106 = ev.Player.ScpController.Scp106;
                        scp106.CapturePlayer(testDummy.Player);
                        break;
                    case RoleTypeId.Scp079:
                        var scp079 = ev.Player.ScpController.Scp079;
                        scp079.RegenEnergy = 200;

                        scp079.Level = 3;//TODO
                        break;
                    case RoleTypeId.Scp096:
                        var scp096 = ev.Player.ScpController.Scp096;
                        scp096.CurentShield = 10;
                        scp096.MaxShield = 100;
                        scp096.ShieldRegeneration = 2000;
                        break;
                }
                break;

            case KeyCode.Alpha5:
                Logger.Warn("Horizontal: " + ev.Player.RotationHorizontal + " Vertical: " + ev.Player.RotationVertical + " Euler" + ev.Player.Rotation.eulerAngles + " Point: " + _state.Position.GetMapRotation().eulerAngles.y);
                break;

            case KeyCode.Alpha6:
                testDummy.ShowPlayer(ev.Player);
                break;

            case KeyCode.Alpha7:
                Schematic?.Destroy();
                Schematic = Synapse.Get<SchematicService>().SpawnSchematic(2000, ev.Player.Position);
                break;

            case KeyCode.Alpha8:
                Schematic.HideFromPlayer(ev.Player);
                break;

            case KeyCode.Alpha9:
                Schematic.ShowPlayer(ev.Player);
                break;
        }
    }
}
#endif
