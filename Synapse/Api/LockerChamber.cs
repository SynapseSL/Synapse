using Interactables.Interobjects.DoorUtils;
using UnityEngine;

namespace Synapse.Api
{
    public class LockerChamber
    {
        internal LockerChamber(MapGeneration.Distributors.LockerChamber vanilalockerChamber, Locker locker, ushort id)
        {
            lockerChamber = vanilalockerChamber;
            vanillalocker = locker.locker;
            Locker = locker;
            colliderId = id;
            byteId = (ushort)(1 << id);
        }

        private readonly MapGeneration.Distributors.Locker vanillalocker;

        public readonly ushort byteId;

        public readonly ushort colliderId;

        public readonly MapGeneration.Distributors.LockerChamber lockerChamber;

        public KeycardPermissions RequiredPermissions
        {
            get => lockerChamber.RequiredPermissions;
            set => lockerChamber.RequiredPermissions = value;
        }

        public Locker Locker { get; }

        public GameObject GameObject 
            => lockerChamber.gameObject;

        public string Name
            => GameObject.name;

        public bool CanInteract
            => lockerChamber.CanInteract;

        public Vector3 Position
            => GameObject.transform.position;

        public bool Open
        {
            get => (vanillalocker.OpenedChambers & byteId) == byteId;
            set
            {
                lockerChamber.IsOpen = value;
                vanillalocker.RefreshOpenedSyncvar();
                vanillalocker.OpenedChambers = value ? (ushort)(vanillalocker.OpenedChambers | byteId) : (ushort)(vanillalocker.OpenedChambers & (~byteId));
                lockerChamber._targetCooldown = 1f;
                lockerChamber._stopwatch.Restart();
            }
        }

        public void SpawnItem(ItemType type, int amount = 1)
            => lockerChamber.SpawnItem(type, amount);
    }
}
