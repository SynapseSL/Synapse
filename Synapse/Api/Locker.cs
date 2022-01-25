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

        public Vector3 Position => GameObject.transform.position;

        public List<LockerChamber> Chambers { get; } = new List<LockerChamber>();

        public Enum.LockerType LockerType
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
