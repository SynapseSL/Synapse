

using System.Collections.ObjectModel;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface IElevator
{
    public string Name { get; }
    
    public int Id { get; }

    public bool Locked { get; set; }
    
    public bool IsMoving { get; }

    public IElevatorDestination CurrentDestination { get; }
    
    public ReadOnlyCollection<IElevatorDestination> Destinations { get; }

    public void MoveToDestination(int destinationId);

    public void MoveToNext();
}