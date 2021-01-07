using UnityEngine;
using Interactables.Interobjects.DoorUtils;
using vDoor = Interactables.Interobjects.DoorUtils.DoorVariant;
using Interactables.Interobjects;

namespace Synapse.Api
{
    public class Door
    {
        internal Door(vDoor vanilladoor) => VDoor = vanilladoor;

        public vDoor VDoor { get; internal set; }

        public GameObject GameObject => VDoor.gameObject;

        public string Name => string.IsNullOrWhiteSpace(VDoor.name) ? GameObject.name : VDoor.name;

        public Vector3 Position => GameObject.transform.position;

        public DoorPermissions DoorPermissions { get => VDoor.RequiredPermissions; set => VDoor.RequiredPermissions = value; }

        private Enum.DoorType doorType;
        public Enum.DoorType DoorType
        {
            get
            {
                if (VDoor != null)
                    return doorType;

                foreach (var type in (Enum.DoorType[])System.Enum.GetValues(typeof(Enum.DoorType)))
                {
                    if (type.ToString().ToUpper().Contains(Name.ToUpper()))
                    {
                        doorType = type;
                        return doorType;
                    }
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

        public bool IsBreakable => VDoor is BreakableDoor;
        public bool IsOpen { get => VDoor.IsConsideredOpen(); }

        public bool TryBreakDoor()
        {
            if (VDoor is BreakableDoor damageableDoor)
            {
                damageableDoor.IsDestroyed = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool TryPry()
        {
            if (VDoor is PryableDoor pry)
                return pry.TryPryGate();
            else
                return false;
        }
    }
}
