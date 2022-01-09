using Interactables.Interobjects.DoorUtils;
using UnityEngine;

namespace Synapse.Api
{
    public class LockerChamber
    {
        internal LockerChamber(MapGeneration.Distributors.LockerChamber vanilalockerChamber, Locker locker, ushort id)
        {
            lockerChamber = vanilalockerChamber;
            this.locker = locker.locker;
            this.Locker = locker; 
            colliderId = id;
            byteId = (ushort)(1 << id);
        }

        private readonly MapGeneration.Distributors.Locker locker;

        public readonly ushort byteId;

        public readonly ushort colliderId;

        public readonly MapGeneration.Distributors.LockerChamber lockerChamber;

        public KeycardPermissions RequiredPermissions
        {
            get => lockerChamber.RequiredPermissions;
            set => lockerChamber.RequiredPermissions = value;
        }

        public Locker Locker { get; }

        public GameObject GameObject => lockerChamber.gameObject;

        public string Name => GameObject.name;

        public bool CanInteract => lockerChamber.CanInteract;

        public Vector3 Position => GameObject.transform.position;

        public bool Open 
        {
            get => (locker.OpenedChambers & byteId) == byteId;
            set
            {

                lockerChamber.IsOpen = value;
                locker.RefreshOpenedSyncvar();
                if (value)
                    locker.OpenedChambers = (ushort)(locker.OpenedChambers | byteId);
                else
                    locker.OpenedChambers = (ushort)(locker.OpenedChambers & (~byteId));
                lockerChamber._targetCooldown = 1f;
                lockerChamber._stopwatch.Restart();
            }
        }
    }
}
