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

        private Enum.DoorType doorType;

        public Enum.DoorType DoorType
        {
            get
            {
                if (door != null)
                    return doorType;

                foreach(var type in (Enum.DoorType[])System.Enum.GetValues(typeof(Enum.DoorType)))
                    if (type.ToString().ToUpper().Contains(Name.ToUpper()))
                    {
                        doorType = type;
                        return doorType;
                    }

                if (Name.Contains("Airlocks")) doorType = Enum.DoorType.Airlock;

                else if (Name.Contains("EntrDoor")) doorType = Enum.DoorType.EZ_Door;

                else if (Name.Contains("LightContainmentDoor")) doorType = Enum.DoorType.LCZ_Door;

                else if (Name.Contains("HeavyContainmentDoor")) doorType = Enum.DoorType.HCZ_Door;

                else if (Name.Contains("PrisonDoor")) doorType = Enum.DoorType.PrisonDoor;

                //else if (Name.Contains("ContDoor")) doorType = Enum.DoorType.ContDoor;

                else doorType = Enum.DoorType.Other;

                return doorType;
            }
        }
    }
}
