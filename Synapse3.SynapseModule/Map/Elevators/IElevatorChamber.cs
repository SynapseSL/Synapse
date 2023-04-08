using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface IElevatorChamber
{
    public Transform ParentTransform { get; }
    
    public Vector3 Position { get; }
    
    public bool IsMoving { get; }
    
    public IElevator MainElevator { get; }
}