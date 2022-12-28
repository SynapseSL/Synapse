using Interactables.Interobjects;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface IElevator
{
    public uint ElevatorId { get; }
    
    public IElevatorChamber Chamber { get; }
    
    public IElevatorDestination[] Destinations { get; }
    
    public IElevatorDestination CurrentDestination { get; }

    public void MoveToDestination(uint destinationId);

    public void MoveToNext();
}