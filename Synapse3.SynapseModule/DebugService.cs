using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;
    private MapEvents _map;
    private RoundEvents _round;
    private ItemEvents _item;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
    }

    private SchematicDestination _destination;

    public override void Enable()
    {
        _map.Scp914Upgrade.Subscribe(Upgrade);
        _map.DoorInteract.Subscribe(OnDoor);
        _player.KeyPress.Subscribe(OnKeyPress);
        _round.SelectTeam.Subscribe(SelectTeam);
        _round.SpawnTeam.Subscribe(SpawnTeam);
        
        _item.KeyCardInteract.Subscribe(KeyCardItem);
        _item.BasicInteract.Subscribe(BasicItem);

        /*_round.Start.Subscribe(_ =>
            _destination =
                new SchematicDestination(Synapse.Get<SchematicService>().SpawnSchematic(8, new Vector3(0f, 1002f, 0f)),
                    99, "test", null));
        
        _map.ElevatorMoveContent.Subscribe(Elevator);
        */
    }

    private void Elevator(ElevatorMoveContentEvent ev)
    {
        if(ev.Elevator.Id != (int)ElevatorType.GateB || ev.Destination.ElevatorId != 0) return;
        ev.Destination = _destination;
        ev.OpenDoorManually = true;
    }

    public override void Disable()
    {
        _map.Scp914Upgrade.Unsubscribe(Upgrade);
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

    private void Upgrade(Scp914UpgradeEvent ev)
    {
        ev.MoveItems = false;
        ev.MovePlayers = false;
    }

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                var comp = ev.Player.GetComponentInChildren<HitboxIdentity>();
                NeuronLogger.For<Synapse>().Warn(comp == null);
                break;
        }
    }
}
#endif