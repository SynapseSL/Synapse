using System.Collections.Generic;
using Interactables.Interobjects;

namespace Synapse3.SynapseModule.Map.Elevators;

public class SynapseElevator : IElevator
{
    public SynapseElevator(ElevatorChamber chamber)
    {
        Chamber = new SynapseElevatorChamber(chamber, this);
        ElevatorId = (uint)chamber._assignedGroup;
        ElevatorGroup = chamber._assignedGroup;

        if (!ElevatorDoor.AllElevatorDoors.TryGetValue(chamber._assignedGroup, out var doors)) return;
        var doorList = new List<IElevatorDestination>();
        foreach (var door in doors)
        {
            doorList.Add(new SynapseElevatorDestination(door, this));
        }

        Destinations = doorList.ToArray();
    }
    
    public ElevatorManager.ElevatorGroup ElevatorGroup { get; }

    public uint ElevatorId { get; }
    public IElevatorChamber Chamber { get; }
    public IElevatorDestination[] Destinations { get; }

    public IElevatorDestination CurrentDestination =>
        Destinations[(Chamber as SynapseElevatorChamber)?.Chamber.CurrentLevel ?? 0];

    public void MoveToDestination(uint destinationId) =>
        ElevatorManager.TrySetDestination((ElevatorManager.ElevatorGroup)ElevatorId, (int)destinationId);

    public void MoveToNext()
    {
        if (Chamber is not SynapseElevatorChamber chamber) return;
        var nextLevel = chamber.Chamber.CurrentLevel + 1;
        if (nextLevel >= Destinations.Length) nextLevel = 0;
        ElevatorManager.TrySetDestination((ElevatorManager.ElevatorGroup)ElevatorId, nextLevel);
    }
}