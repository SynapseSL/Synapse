using UnityEngine;
using System.Linq;

namespace Synapse.Api
{
    public class Door
    {
        internal Door(global::Door vanilladoor) => door = vanilladoor;

        private global::Door door;

        public GameObject GameObject => door.gameObject;

        public string Name => GameObject.name;

        public Vector3 Position => door.localPos;

        public bool Open { get => door.isOpen; set => door.SetState(value); }

        public bool Locked { get => door.locked; set => door.SetLock(value); }

        public Room Room { get => Map.Get.Rooms.OrderBy(x => Vector3.Distance(x.Position, Position)).FirstOrDefault(); }

        public global::Door.AccessRequirements PermissionLevels { get => door.PermissionLevels; set => door.PermissionLevels = value; }

        public global::Door.DoorStatus Status => door.status;
    }
}
