﻿using System.IO;
using GameObjectPools;
using InventorySystem.Items.MicroHID;
using LiteNetLib.Utils;
using MEC;
using Mirror;
using Neuron.Core;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Modules.Commands;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using RelativePositioning;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Synapse3.SynapseModule.Dummy;
using GameCore;
using PluginAPI.Core;
using System.Diagnostics;
using PluginAPI.Events;
using PluginAPI.Core.Interfaces;
using InventorySystem.Items.Firearms;
using PluginAPI.Core.Attributes;
using Synapse3.SynapseModule.Permissions;

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
        
        _item.Shoot.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Shoot {ev.Player.NickName} {ev.Target?.NickName} {ev.Item.ItemType}");
        });

        _scp.Revive.Subscribe(ev =>
        {
           
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

        _scp.ContainScp079.Subscribe(ev => NeuronLogger.For<Synapse>().Warn("Contain 079: " + ev.Status));

        _scp.Scp079DoorInteract.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn("079 Door");
        });

        _scp.Revive.Subscribe(ev => ev.Allow = false);

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
        

        _player.Damage.Subscribe(ev =>
        {
            ev.Allow = false;
        });

        PluginAPI.Events.EventManager.RegisterEvents(typeof(DebugService), this);//Temp to attach event of NW
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
    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                testDummy?.Destroy();
                testDummy = new SynapseDummy(ev.Player.Position, ev.Player.Rotation, RoleTypeId.ClassD, "Test");
                testDummy.Player.RoleType = RoleTypeId.NtfSergeant;
                testDummy.Player.Position = ev.Player.Position;
                testDummy.Movement = PlayerMovementState.Crouching;
                var service = Synapse.Get<PermissionService>();
                var grouype = service.Groups["User"];
                testDummy.Player.SynapseGroup = grouype;
                break;
           
            case KeyCode.Alpha2:
                testDummy.Player.RotationHorizontal = 50;
                testDummy.Player.RotationVectical = 50; 
                break;

            case KeyCode.Alpha3:
                testDummy.Direction = MovementDirection.Forward;
            break;

            case KeyCode.Alpha4:
                testDummy.RaVisible = !testDummy.RaVisible;
                break;

            case KeyCode.Alpha5:
                testDummy.HideFromPlayer(ev.Player);
                break;

            case KeyCode.Alpha6:
                testDummy.ShowPlayer(ev.Player);
                break;

        }
    }
}
#endif
