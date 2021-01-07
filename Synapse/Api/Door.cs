using UnityEngine;
using Interactables.Interobjects.DoorUtils;
using vDoor = Interactables.Interobjects.DoorUtils.DoorVariant;
using Interactables.Interobjects;

namespace Synapse.Api
{
    public class Door
    {
        internal Door(vDoor vanilladoor)
        {
            VDoor = vanilladoor;
            if (VDoor.TryGetComponent<DoorNametagExtension>(out var nametag))
                Name = nametag.GetName;
        }

        public vDoor VDoor { get; internal set; }

        public GameObject GameObject => VDoor.gameObject;

        private string name;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return GameObject.name;
                return name;
            }
            set => name = value;
        }

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

        public override string ToString() => Name;
    }
}
