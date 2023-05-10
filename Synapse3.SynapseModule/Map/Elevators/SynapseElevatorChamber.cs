using Interactables.Interobjects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class SynapseElevatorChamber : IElevatorChamber
{
    public SynapseElevatorChamber(ElevatorChamber chamber, SynapseElevator elevator)
    {
        Chamber = chamber;
        MainElevator = elevator;
    }
    public ElevatorChamber Chamber { get; }

    public Transform ParentTransform => Chamber.transform;
    public Vector3 Position => Chamber.transform.position;
    public Quaternion Rotation => Chamber.transform.rotation;
    public bool IsMoving => Chamber._curSequence == ElevatorChamber.ElevatorSequence.MovingAway;
    public IElevator MainElevator { get; }
}