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

            DoorType = DetermineDoorType();

            Rooms = new List<Room>();

            DoorType DetermineDoorType()
            {
                foreach (var type in (DoorType[])System.Enum.GetValues(typeof(DoorType)))
                {
                    if (type.ToString().ToUpper().Contains(Name.ToUpper()))
                    {
                        return type;
                    }
                }

                if (Name.Contains("EZ BreakableDoor"))
                {
                    return DoorType.EZ_Door;
                }
                else if (Name.Contains("LCZ BreakableDoor"))
                {
                    return DoorType.LCZ_Door;
                }
                else if (Name.Contains("HCZ BreakableDoor"))
                {
                    return DoorType.HCZ_Door;
                }
                else if (Name.Contains("Prison BreakableDoor"))
                {
                    return DoorType.PrisonDoor;
                }
                else if (Name.Contains("LCZ PortallessBreakableDoor"))
                {
                    return DoorType.Airlock;
                }
                else if (Name.Contains("Unsecured Pryable GateDoor"))
                {
                    return DoorType.HCZ_049_Gate;
                }
                else
                {
                    return DoorType.Other;
                }
            }
        }

        public vDoor VDoor { get; internal set; }

        public GameObject GameObject
            => VDoor.gameObject;

        private string name;
        public string Name
        {
            get => name ?? GameObject.name;
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

        public DoorPermissions DoorPermissions
        {
            get => VDoor.RequiredPermissions;
            set => VDoor.RequiredPermissions = value;
        }

        public DoorType DoorType { get; }

        public bool IsBreakable
            => VDoor is BreakableDoor;

        public bool IsDestroyed
            => VDoor is BreakableDoor bd && bd.IsDestroyed;

        public bool IsPryable
            => VDoor is PryableDoor;

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
            => VDoor is PryableDoor pry && pry.TryPryGate();

        public List<Room> Rooms { get; }

        [Obsolete("Please create a Synapse.Api.CustomObjects.SynapseDoorObject")]
        public static Door SpawnDoorVariant(Vector3 position, Quaternion? rotation = null, DoorPermissions permissions = null)
        {
            if (rotation is null)
                rotation = Quaternion.identity;
            var obj = new SynapseDoorObject(SpawnableDoorType.HCZ, position, rotation.Value, Vector3.one);
            obj.Door.DoorPermissions = permissions;
            return obj.Door;
        }

        public override string ToString()
            => Name;
    }
}
