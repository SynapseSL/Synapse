using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Synapse.Api.CustomObjects;
using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
using vDoor = Interactables.Interobjects.DoorUtils.DoorVariant;

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

        public GameObject GameObject => VDoor?.gameObject;

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
                GameObject.transform.position = value;
                VDoor.netIdentity.DespawnForAllPlayers();
                VDoor.netIdentity.UpdatePositionRotationScale();
            }
        }

        public Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set
            {
                GameObject.transform.rotation = value;
                VDoor.netIdentity.UpdatePositionRotationScale();
            }
        }

        public Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                GameObject.transform.localScale = value;
                VDoor.netIdentity.UpdatePositionRotationScale();
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

        public bool IsDestroyed => VDoor is BreakableDoor bd && bd.IsDestroyed;

        public bool IsPryable => VDoor is PryableDoor;

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

        [Obsolete("Please create a Synapse.Api.CustomObjects.SynapseDoorObject")]
        public static Door SpawnDoorVariant(Vector3 position, Quaternion? rotation = null, DoorPermissions permissions = null)
        {
            if(rotation == null) rotation = Quaternion.identity;
            var obj = new SynapseDoorObject(SpawnableDoorType.HCZ, position, rotation.Value, Vector3.one);
            obj.Door.DoorPermissions = permissions;
            return obj.Door;
        }

        public override string ToString() => Name;
    }
}
