using Interactables.Interobjects;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class SynapseElevatorDestination : IElevatorDestination
{
    public SynapseElevatorDestination(ElevatorDoor door, SynapseElevator elevator)
    {
        Door = door;
        SynapseDoor = door.GetSynapseDoor();
        MainElevator = elevator;
    }
    
    public ElevatorDoor Door { get; }
    public SynapseDoor SynapseDoor { get; }

    public Vector3 Position => SynapseDoor.Position;
    public Quaternion Rotation => SynapseDoor.Rotation;

    public bool Open
    {
        get => SynapseDoor.Open;
        set => SynapseDoor.Open = value;
    }
    
    public IElevator MainElevator { get; }
}