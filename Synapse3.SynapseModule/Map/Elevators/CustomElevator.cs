using System.Collections.Generic;
using System.Linq;
using MEC;
using Neuron.Core.Logging;

namespace Synapse3.SynapseModule.Map.Elevators;

public abstract class CustomElevator : DefaultElevator
{
    public override string Name { get; }
    public override int Id { get; }
    public override bool Locked { get; set; }
    private bool _moving;
    public override bool IsMoving => _moving;

    public override void MoveToDestination(int destinationId)
    {
        NeuronLogger.For<Synapse>().Warn("Move Custom Object");
        if (Destinations.Any(x => x.ElevatorId == destinationId))
        {
            NeuronLogger.For<Synapse>().Warn("Check");
            _moving = true;
            Timing.RunCoroutine(MoveTo(destinationId));
        }
    }

    public virtual float MoveContentDelay { get; } = 3f;

    public virtual float ReOpenDoorsDelay{ get; } = 4f;

    protected virtual void CloseAllDestinations()
    {
        foreach (var destination in Destinations)
        {
            destination.Open = false;
            destination.Locked = true;
        }
    }

    protected virtual void OpenDestination(int destinationId)
    {
        var destination = GetDestination(destinationId);
        destination.Open = true;
    }

    protected virtual IEnumerator<float> MoveTo(int destinationId)
    {
        CloseAllDestinations();
        yield return Timing.WaitForSeconds(MoveContentDelay);
        MoveContent(destinationId);
        yield return Timing.WaitForSeconds(ReOpenDoorsDelay);
        OpenDestination(destinationId);
        _moving = false;
        CurrentDestination = GetDestination(destinationId);
    }
}