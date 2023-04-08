using System.Collections.Generic;
using System.Collections.ObjectModel;
using Interactables.Interobjects;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class ElevatorService : Service
{
    private readonly RoundEvents _round;
    private readonly MapEvents _map;

    public ElevatorService(RoundEvents round, MapEvents map)
    {
        _round = round;
        _map = map;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(GetVanillaElevators);
        _round.Restart.Subscribe(Clear);
        ElevatorChamber.OnElevatorMoved += MoveVanillaContent;
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(GetVanillaElevators);
        _round.Restart.Unsubscribe(Clear);
        ElevatorChamber.OnElevatorMoved -= MoveVanillaContent;
    }

    internal List<IElevator> _elevators = new ();
    public ReadOnlyCollection<IElevator> Elevators => _elevators.AsReadOnly();

    private void GetVanillaElevators(RoundWaitingEvent _)
    {
        foreach (var elevator in ElevatorManager.SpawnedChambers)
        {
            _elevators.Add(new SynapseElevator(elevator.Value));
        }
    }

    private void Clear(RoundRestartEvent _)
    {
        _elevators.Clear();
    }

    private void MoveVanillaContent(Bounds elevatorBounds, ElevatorChamber chamber, Vector3 deltaPos,
        Quaternion deltaRot)
    {
        _map.ElevatorMoveContent.RaiseSafely(new ElevatorMoveContentEvent(chamber.GetSynapseElevator(), deltaPos,
            deltaRot, elevatorBounds));
    }
}