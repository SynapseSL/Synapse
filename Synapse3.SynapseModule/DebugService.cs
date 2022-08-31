using System;
using System.Collections.Generic;
using InventorySystem.Items.MicroHID;
using MEC;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Command.SynapseCommands;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Rooms;
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
            NeuronLogger.For<Synapse>()
                .Warn($"Revive {ev.Scp049.NickName} {ev.HumanToRevive.NickName} {ev.Ragdoll.RoleType} Finish: {ev.FinishRevive}");
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
            NeuronLogger.For<Synapse>().Warn($"{ev.Player.NickName} {ev.DamageType} {ev.LastTakenDamage}");
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
            NeuronLogger.For<Synapse>().Warn($"Pre Auth {ev.UserId}");
        });
        
        _scp.Scp049Attack.Subscribe(ScpEvent);
        _scp.Scp0492Attack.Subscribe(ScpEvent);
        _scp.Scp173Attack.Subscribe(ScpEvent);
        _scp.Scp096Attack.Subscribe(ScpEvent);
        _scp.Scp939Attack.Subscribe(ScpEvent);
        _scp.Scp106Attack.Subscribe(ScpEvent);

        _scp.Revive.Subscribe(ev => ev.Allow = false);

        Synapse.Get<SynapseObjectEvents>().ButtonPressed
            .Subscribe(ev =>
            {
                NeuronLogger.For<Synapse>().Warn("Button Pressed " + ev.ButtonId);
                ev.Player.SendBroadcast("You pressed me!", 5);
            });
    }

    public override void Disable()
    {
        _player.KeyPress.Unsubscribe(OnKeyPress);
    }

    private void ScpEvent(ScpAttackEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn($"{ev.ScpAttackType} {ev.Damage} {ev.Scp.NickName} {ev.Victim.NickName}");
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
        ev.TeamId = 15;
        NeuronLogger.For<Synapse>().Warn("Team Selected");
    }

    private void SpawnTeam(SpawnTeamEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("SpawnTeam: " + ev.TeamId);
    }

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                for (int i = 0; i < 51; i++)
                {
                    NeuronLogger.For<Synapse>().Warn(i + " " + LayerMask.LayerToName(i));
                }
                break;
        }
    }

    private IEnumerator<float> Test(SynapsePlayer player)
    {
        for (int i = 0; i < 479; i++)
        {
            player.ExecuteCommand("forceclass 2 2");
            player.ExecuteCommand("server_event force_mtf_respawn");
            yield return Timing.WaitForSeconds(0.2f);
        }
        Logger.Warn("DONE");
        
        yield break;
    }
}
#endif
