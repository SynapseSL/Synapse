using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    private List<IElevator> _elevators = new ();
    public ReadOnlyCollection<IElevator> Elevators => _elevators.AsReadOnly();

    public void AddElevator(IElevator elevator)
    {
        _elevators.Add(elevator);
    }

    private void GetVanillaElevators(RoundWaitingEvent _)
    {
        foreach (var lift in Synapse.GetObjects<Lift>())
        {
            _elevators.Add(new SynapseElevator(lift));
        }
    }

    private void Clear(RoundRestartEvent _)
    {
        _elevators.Clear();
    }
}