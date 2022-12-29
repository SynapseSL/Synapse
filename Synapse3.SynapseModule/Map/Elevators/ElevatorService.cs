using System.Collections.Generic;
using System.Collections.ObjectModel;
using Interactables.Interobjects;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Map.Elevators;

public class ElevatorService : Service
{
    private RoundEvents _round;

    public ElevatorService(RoundEvents round)
    {
        _round = round;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(GetVanillaElevators);
        _round.Restart.Subscribe(Clear);
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(GetVanillaElevators);
        _round.Restart.Unsubscribe(Clear);
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
}