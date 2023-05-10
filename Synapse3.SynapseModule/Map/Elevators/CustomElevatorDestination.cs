using System.Linq;
using Ninject;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class CustomElevatorDestination : IElevatorDestination
{
    [Inject]
    public SchematicService SchematicService { get; set; }
    
    
    public SynapseSchematic Schematic { get; private set; }
    public Vector3 Position => Schematic.Position;
    public Quaternion Rotation => Schematic.Rotation;

    public bool Open
    {
        get => Schematic.Doors.First()?.Open ?? true;
        set
        {
            foreach (var door in Schematic.Doors)
            {
                door.Open = value;
            }
        }
    }
    public IElevator MainElevator { get; private set; }

    public void Generate(IElevator elevator,uint schematicId, Vector3 position, Quaternion rotation)
    {
        MainElevator = elevator;
        Schematic = SchematicService.SpawnSchematic(schematicId, position, rotation);
    }
}