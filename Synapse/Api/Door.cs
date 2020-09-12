using UnityEngine;

namespace Synapse.Api
{
    public class Door
    {
        internal Door(global::Door vanilladoor) => door = vanilladoor;

        private global::Door door;

        public GameObject GameObject => door.gameObject;

        public bool Open { get => door.isOpen; set => door.SetState(value); }

        public bool Locked { get => door.locked; set => door.SetLock(value); }

        public global::Door.AccessRequirements PermissionLevels { get => door.PermissionLevels; set => door.PermissionLevels = value; }
    }
}
