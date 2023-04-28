using Ninject;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class CustomElevatorChamber : IElevatorChamber
{
    [Inject]
    public SchematicService SchematicService { get; set; }
    
    public SynapseSchematic Schematic { get; private set; }
    public Transform ParentTransform => Schematic.GameObject.transform;
    public Vector3 Position => Schematic.Position;
    public Quaternion Rotation => Schematic.Rotation;
    public bool IsMoving { get; } = false;
    public IElevator MainElevator { get; private set; }

    public void Generate(CustomElevator elevator,uint schematicId,CustomElevatorDestination destination)
    {
        MainElevator = elevator;
        Schematic = SchematicService.SpawnSchematic(schematicId, destination.Position, destination.Rotation);
    }
}