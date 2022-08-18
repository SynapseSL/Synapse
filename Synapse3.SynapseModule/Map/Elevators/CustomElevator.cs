using System.Collections.Generic;
using System.Linq;
using MEC;

namespace Synapse3.SynapseModule.Map.Elevators;

public abstract class CustomElevator : DefaultElevator
{
    public ElevatorService ElevatorService;
    
    public override string Name { get; }
    public override uint Id { get; }
    public override bool Locked { get; set; }
    private bool _moving;
    public override bool IsMoving => _moving;

    public CustomElevator()
    {
        ElevatorService = Synapse.Get<ElevatorService>();
        ElevatorService._elevators.Add(this);
    }

    public override void MoveToDestination(uint destinationId)
    {
        if (Destinations.Any(x => x.ElevatorId == destinationId) && !_moving)
        {
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

    protected virtual void OpenDestination(uint destinationId)
    {
        var destination = GetDestination(destinationId);
        destination.Open = true;
    }

    protected virtual IEnumerator<float> MoveTo(uint destinationId)
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