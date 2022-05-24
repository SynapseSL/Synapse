using System;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public class SchematicDoor : AttributeHandler
    {
        public override string Name => "SchematicDoor";

        public override void OnLoad(ISynapseObject synapseObject, ArraySegment<string> args)
        {
            if (!(synapseObject is SynapseDoorObject door))
                return;
            if (args.Count == 0 || !Int32.TryParse(args.At(0), out var leftschematicID) || !SchematicHandler.Get.IsIDRegistered(leftschematicID))
                return;

            var leftDoor = door.GameObject.transform.GetChild(2).GetChild(1).GetChild(0);
            var leftschematic = SchematicHandler.Get.SpawnSchematic(leftschematicID, leftDoor.transform.position);
            leftschematic.GameObject.transform.parent = leftDoor.transform;

            if (door.DoorType != Enum.SpawnableDoorType.EZ)
            {
                if (args.Count < 2 || !Int32.TryParse(args.At(1), out var righttschematicID) || !SchematicHandler.Get.IsIDRegistered(righttschematicID))
                    return;

                var rightDoor = door.GameObject.transform.GetChild(2).GetChild(1).GetChild(1);
                var rightschematic = SchematicHandler.Get.SpawnSchematic(righttschematicID, rightDoor.transform.position);
                rightschematic.GameObject.transform.parent = rightDoor.transform;
            }
        }
    }
}
