using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface IElevatorDestination
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
    
    public bool Open { get; set; }
    
    public IElevator MainElevator { get; }
}