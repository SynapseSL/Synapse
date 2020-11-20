using UnityEngine;
using System.Linq;

namespace Synapse.Api
{
    public class Door
    {
        internal Door(global::Door vanilladoor) => door = vanilladoor;

        private global::Door door;

        public GameObject GameObject => door.gameObject;

        public string Name => string.IsNullOrWhiteSpace(door.DoorName) ? GameObject.name : door.DoorName;

        public Vector3 Position => door.localPos;

        public bool Open { get => door.isOpen; set => door.SetState(value); }

        public bool Locked { get => door.locked; set => door.SetLock(value); }

        public Room Room { get => Map.Get.Rooms.OrderBy(x => Vector3.Distance(x.Position, Position)).FirstOrDefault(); }

        public global::Door.AccessRequirements PermissionLevels { get => door.PermissionLevels; set => door.PermissionLevels = value; }

        public global::Door.DoorStatus Status => door.status;

        public Enum.DoorType DoorType
        {
            get
            {
                foreach(var type in (Enum.DoorType[])System.Enum.GetValues(typeof(Enum.DoorType)))
                    if (type.ToString().ToUpper().Contains(Name.ToUpper()))
                        return type;

                if (Name.Contains("Airlocks")) return Enum.DoorType.Airlock;

                if (Name.Contains("EntrDoor")) return Enum.DoorType.EZ_Door;

                if (Name.Contains("LightContainmentDoor")) return Enum.DoorType.LCZ_Door;

                if (Name.Contains("HeavyContainmentDoor")) return Enum.DoorType.HCZ_Door;

                if (Name.Contains("PrisonDoor")) return Enum.DoorType.PrisonDoor;

                //if (Name.Contains("ContDoor")) return Enum.DoorType.Other;

                return Enum.DoorType.Other;
            }
        }
    }
}
