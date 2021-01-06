using UnityEngine;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using vDoor = Interactables.Interobjects.DoorUtils.DoorVariant;

namespace Synapse.Api
{
    public class Door
    {
        internal Door(vDoor vanilladoor) => door = vanilladoor;

        internal vDoor door;

        public GameObject GameObject => door.gameObject;

        public string Name => string.IsNullOrWhiteSpace(door.name) ? GameObject.name : door.name;

        public Vector3 Position => GameObject.transform.position;

        public DoorPermissions DoorPermissions { get => door.RequiredPermissions; set => door.RequiredPermissions = value; }

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
