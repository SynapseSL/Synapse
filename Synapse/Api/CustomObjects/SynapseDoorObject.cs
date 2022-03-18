using Interactables.Interobjects;
using Mirror;
using Synapse.Api.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseDoorObject : NetworkSynapseObject
    {
        public static Dictionary<SpawnableDoorType, BreakableDoor> Prefab { get; internal set; } = new Dictionary<SpawnableDoorType, BreakableDoor>();

        internal SynapseDoorObject(SynapseSchematic.DoorConfiguration configuration)
        {
            Door = CreateDoor(configuration.DoorType, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale, configuration.Open, configuration.Locked);
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;
            UpdateEveryFrame = configuration.UpdateEveryFrame;
            DoorType = configuration.DoorType;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public SynapseDoorObject(SpawnableDoorType type, Vector3 position, Quaternion rotation, Vector3 scale, bool open = false, bool locked = false)
        {
            Door = CreateDoor(type, position, rotation, scale, open, locked);
            DoorType = type;

            Map.Get.SynapseObjects.Add(this);
            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override GameObject GameObject => Door.GameObject;
        public override NetworkIdentity NetworkIdentity => Door.VDoor.netIdentity;
        public override ObjectType Type => ObjectType.Door;

        public Door Door { get; }
        public SpawnableDoorType DoorType { get; }
        public bool Open
        {
            get => Door.Open;
            set => Door.Open = value;
        }
        public bool Locked
        {
            get => Door.Locked;
            set => Door.Locked = value;
        }

        private Door CreateDoor(SpawnableDoorType type, Vector3 position, Quaternion rotation, Vector3 scale, bool open, bool locked)
        {
            var ot = CreateNetworkObject(Prefab[type], position, rotation, scale);
            var door = new Door(ot);
            door.Open = open;
            door.Locked = locked;
            return door;
        }
    }
}
