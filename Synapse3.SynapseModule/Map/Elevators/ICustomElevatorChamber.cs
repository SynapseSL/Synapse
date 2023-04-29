using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public interface ICustomElevatorChamber : IElevatorChamber
{
    public void Update();

    public void MoveToLocation(Vector3 position, Quaternion rotation);
}