using System;
using Synapse3.SynapseModule.Map.Objects;

namespace Synapse3.SynapseModule.Map.Schematic.CustomAttributes;

public class SchematicDoor : AttributeHandler
{
    private SchematicService _schematic;

    public SchematicDoor()
    {
        _schematic = Synapse.Get<SchematicService>();
    }
    
    public override string Name => "SchematicDoor";

    public override void OnLoad(ISynapseObject synapseObject, ArraySegment<string> args)
    {
        if(!(synapseObject is SynapseDoor door)) return;
        if (args.Count == 0 || !int.TryParse(args.At(0), out var leftSchematicID) || !_schematic.IsIDRegistered(leftSchematicID)) return;

        var leftDoor = door.GameObject.transform.GetChild(2).GetChild(1).GetChild(0);
        var leftSchematic = _schematic.SpawnSchematic(leftSchematicID, leftDoor.transform.position, door.Rotation);
        leftSchematic.GameObject.transform.parent = leftDoor.transform;

        if (door.SpawnableType != SynapseDoor.SpawnableDoorType.Ez)
        {
            if (args.Count < 2 || !int.TryParse(args.At(1), out var righttschematicID) || !_schematic.IsIDRegistered(righttschematicID)) return;

            var rightDoor = door.GameObject.transform.GetChild(2).GetChild(1).GetChild(1);
            var rightSchematic =
                _schematic.SpawnSchematic(righttschematicID, rightDoor.transform.position, door.Rotation);
            rightSchematic.GameObject.transform.parent = rightDoor.transform;
        }
    }
}