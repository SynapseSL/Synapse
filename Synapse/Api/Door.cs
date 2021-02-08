using UnityEngine;
using Interactables.Interobjects.DoorUtils;
using vDoor = Interactables.Interobjects.DoorUtils.DoorVariant;
using Interactables.Interobjects;
using Mirror;
using System.Collections.Generic;

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
                if (string.IsNullOrEmpty(name)) return GameObject.name;
                return name;
            }
            set => name = value;
        }

        public Vector3 Position
        {
            get => GameObject.transform.position;
            set
            {
                NetworkServer.UnSpawn(GameObject);
                GameObject.transform.position = value;
                NetworkServer.Spawn(GameObject);
            }
        }

        public Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set
            {
                NetworkServer.UnSpawn(GameObject);
                GameObject.transform.rotation = value;
                NetworkServer.Spawn(GameObject);
            }
        }

        public DoorPermissions DoorPermissions { get => VDoor.RequiredPermissions; set => VDoor.RequiredPermissions = value; }

        private Enum.DoorType doorType;
        public Enum.DoorType DoorType
        {
            get
            {
                foreach (var type in (Enum.DoorType[])System.Enum.GetValues(typeof(Enum.DoorType)))
                {
                    if (type.ToString().ToUpper().Contains(Name.ToUpper()))
                    {
                        doorType = type;
                        return doorType;
                    }
                }

                if (Name.Contains("EZ BreakableDoor")) doorType = Enum.DoorType.EZ_Door;
                else if (Name.Contains("LCZ BreakableDoor")) doorType = Enum.DoorType.LCZ_Door;
                else if (Name.Contains("HCZ BreakableDoor")) doorType = Enum.DoorType.HCZ_Door;
                else if (Name.Contains("Prison BreakableDoor")) doorType = Enum.DoorType.PrisonDoor;
                else if (Name.Contains("LCZ PortallessBreakableDoor")) doorType = Enum.DoorType.Airlock;
                else if (Name.Contains("Unsecured Pryable GateDoor")) doorType = Enum.DoorType.HCZ_049_Gate;
                else doorType = Enum.DoorType.Other;

                return doorType;
            }
        }

        public bool IsBreakable => VDoor is BreakableDoor;

        public bool Open
        {
            get => VDoor.IsConsideredOpen();
            set => VDoor.NetworkTargetState = value;
        }

        public bool Locked
        {
            get => VDoor.ActiveLocks > 0;
            set => VDoor.ServerChangeLock(DoorLockReason.SpecialDoorFeature, value);
        }

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

        public List<Room> Rooms { get; } = new List<Room>();

        public static Door SpawnDoorVariant(Vector3 position, Quaternion? rotation = null, DoorPermissions permissions = null)
        {
            DoorVariant doorVariant = Object.Instantiate(Server.Get.Prefabs.DoorVariantPrefab);

            doorVariant.transform.position = position;
            doorVariant.transform.rotation = rotation ?? new Quaternion(0, 0, 0, 0);
            doorVariant.RequiredPermissions = permissions ?? new DoorPermissions();
            var door = new Door(doorVariant);
            Map.Get.Doors.Add(door);
            NetworkServer.Spawn(doorVariant.gameObject);

            return door;
        }

        public override string ToString() => Name;
    }
}
