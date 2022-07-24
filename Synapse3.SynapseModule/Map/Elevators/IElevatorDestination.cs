using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface IElevatorDestination
{
    public IElevator Elevator { get; }
    
    public Transform Transform { get; }
    
    public string DestinationName { get; }
    
    public bool Open { get; set; }
    
    public bool Locked { get; set; }
    
    public int ElevatorId { get; }
    

    public Vector3 RangeScale { get; set; }
    
    public Vector3 DestinationPosition { get; set; }
    
    public Vector3 GetWorldPosition(Vector3 localPosition);
    public Vector3 GetLocalPosition(Vector3 worldPosition);
}