using InventorySystem.Items.MicroHID;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Command.SynapseCommands;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Rooms;
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
    private SynapseCommandService _commandService;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item,ScpEvents scp, SynapseCommandService commandService)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
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
        
        _scp.Scp049Attack.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Scp049 Attack {ev.Cooldown} {ev.Damage} {ev.Scp.NickName} {ev.Victim.NickName}");
        });
        
        _item.ThrowGrenade.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Throw {ev.State}");
            ev.Allow = false;
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
    }

    public override void Disable()
    {
        _player.KeyPress.Unsubscribe(OnKeyPress);
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
            case KeyCode.Alpha2:
                var room = (SynapseNetworkRoom)RoomType.Scp939.GetRoom();
                room.Position = ev.Player.Position;
                break;
            
            case KeyCode.Alpha3:
                new SynapseDummy(ev.Player.Position, ev.Player.RotationVector2, ev.Player.RoleType, ev.Player.NickName,
                    "", "");
                break;
        }
    }
}
#endif
