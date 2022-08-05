

using System.Collections.ObjectModel;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface IElevator
{
    public string Name { get; }
    
    public uint Id { get; }

    public bool Locked { get; set; }
    
    public bool IsMoving { get; }

    public IElevatorDestination CurrentDestination { get; }
    
    public ReadOnlyCollection<IElevatorDestination> Destinations { get; }

    public void MoveToDestination(uint destinationId);

    public void MoveToNext();
}