using MEC;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Respawning;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;
    private MapEvents _map;
    private RoundEvents _round;
    private ItemEvents _item;
    private SynapseCommandService _commandService;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, SynapseCommandService commandService)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _commandService = commandService;
    }

    public override void Enable()
    {
        _player.DoorInteract.Subscribe(OnDoor);
        _player.KeyPress.Subscribe(OnKeyPress);
        _round.SelectTeam.Subscribe(SelectTeam);
        _round.SpawnTeam.Subscribe(SpawnTeam);
        
        _item.KeyCardInteract.Subscribe(KeyCardItem);
        _item.BasicInteract.Subscribe(BasicItem);

        _player.Death.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"{ev.Player.NickName} {ev.DamageType} {ev.LastTakenDamage}");
        });
        _player.Damage.Subscribe(ev =>
            NeuronLogger.For<Synapse>().Warn($"Damage: {ev.Player.NickName} {ev.Damage} {ev.DamageType}"));
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
        NeuronLogger.For<Synapse>().Warn("Keycard Use State: " + ev.State);
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
                new SynapseDoor(SynapseDoor.SpawnableDoorType.Hcz, ev.Player.Position, ev.Player.Rotation,
                    Vector3.one)
                {
                    Health = 5000,
                };
                break;
            
            case KeyCode.Alpha2:
                new SynapseDoor(SynapseDoor.SpawnableDoorType.Hcz, ev.Player.Position, ev.Player.Rotation,
                    Vector3.one)
                {
                    UnDestroyable = true
                };
                break;
        }
    }
}
#endif