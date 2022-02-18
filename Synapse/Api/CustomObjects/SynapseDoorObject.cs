using Interactables.Interobjects;
using Mirror;
using Synapse.Api.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseDoorObject : DefaultSynapseObject
    {
        public static Dictionary<SpawnableDoorType, BreakableDoor> Prefab { get; internal set; } = new Dictionary<SpawnableDoorType, BreakableDoor>();

        internal SynapseDoorObject(SynapseSchematic.DoorConfiguration configuration)
        {
            Door = CreateDoor(configuration.DoorType, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale, configuration.Open, configuration.Locked);
            OriginalScale = configuration.Scale;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public SynapseDoorObject(SpawnableDoorType type, Vector3 position, Quaternion rotation, Vector3 scale, bool open = false, bool locked = false)
        {
            Door = CreateDoor(type, position, rotation, scale, open, locked);

            Map.Get.SynapseObjects.Add(this);
            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }


        public override GameObject GameObject => Door.GameObject;
        public override ObjectType Type => ObjectType.Door;
        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                Refresh();
            }
        }
        public override Quaternion Rotation
        {
            get => base.Rotation;
            set
            {
                base.Rotation = value;
                Refresh();
            }
        }
        public override Vector3 Scale
        {
            get => base.Scale;
            set
            {
                base.Scale = value;
                Refresh();
            }
        }

        public Door Door { get; }
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


        public void Refresh()
            => Door.VDoor.netIdentity.UpdatePositionRotationScale();

        private Door CreateDoor(SpawnableDoorType type, Vector3 position, Quaternion rotation, Vector3 scale, bool open, bool locked)
        {
            var ot = UnityEngine.Object.Instantiate(Prefab[type], position, rotation);
            ot.transform.position = position;
            ot.transform.rotation = rotation;
            ot.transform.localScale = scale;
            NetworkServer.Spawn(ot.gameObject);
            var door = new Door(ot);
            door.Open = open;
            door.Locked = locked;
            return door;
        }
    }
}
