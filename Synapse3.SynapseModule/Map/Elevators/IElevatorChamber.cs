using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface IElevatorChamber
{
    public Vector3 Position { get; }
    
    public bool IsMoving { get; set; }
    
    public IElevator MainElevator { get; }
}