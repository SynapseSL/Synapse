using MapGeneration.Distributors;
using Synapse.Api.Enum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Locker
    {
        internal Locker(MapGeneration.Distributors.Locker vanillalocker)
        {
            locker = vanillalocker;
            for (ushort i = 0; i < locker.Chambers.Count(); i++)
                Chambers.Add(new LockerChamber(locker.Chambers[i], this, i));
        }

        public readonly MapGeneration.Distributors.Locker locker;

        public GameObject GameObject => locker.gameObject;

        public string Name => GameObject.name;

        public Vector3 Position
        {
            get => GameObject.transform.position;
            set
            {
                var comp = GameObject.GetComponent<StructurePositionSync>();
                comp.Network_position = value;
                locker.transform.position = value;
                locker.netIdentity.UpdatePositionRotationScale();
            }
        }

        public Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set
            {
                var comp = GameObject.GetComponent<StructurePositionSync>();
                comp.Network_rotationY = (sbyte)Mathf.RoundToInt(locker.transform.rotation.eulerAngles.y / 5.625f);
                locker.transform.rotation = value;
                locker.netIdentity.UpdatePositionRotationScale();
            }
        }

        public Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                locker.transform.localScale = value;
                locker.netIdentity.UpdatePositionRotationScale();
            }
        }

        public List<LockerChamber> Chambers { get; } = new List<LockerChamber>();

        public LockerType LockerType
        {
            get
            {
                if (Name.Contains("AdrenalineMedkit")) return Enum.LockerType.MedkitWallCabinet;
                else if (Name.Contains("RegularMedkit")) return Enum.LockerType.AdrenalineWallCabinet;
                else if (Name.Contains("Pedestal")) return Enum.LockerType.ScpPedestal;
                else if (Name.Contains("MiscLocker")) return Enum.LockerType.StandardLocker;
                else if (Name.Contains("RifleRack")) return Enum.LockerType.RifleRackLocker;
                else if (Name.Contains("LargeGunLocker")) return Enum.LockerType.LargeGunLocker;
                return default;
            }
        }

        public override string ToString() => Name;
    }
}
