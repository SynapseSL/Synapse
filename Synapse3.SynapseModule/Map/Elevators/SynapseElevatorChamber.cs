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
    
    public Vector3 Position { get; }
    public bool IsMoving { get; set; }
    public IElevator MainElevator { get; }
}