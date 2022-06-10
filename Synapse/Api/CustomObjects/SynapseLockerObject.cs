using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Mirror;
using Synapse.Api.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseLockerObject : StructureSyncSynapseObject
    {
        public static Dictionary<LockerType, MapGeneration.Distributors.Locker> Prefabs = new Dictionary<LockerType, MapGeneration.Distributors.Locker>();

        public SynapseLockerObject(LockerType lockerType, Vector3 pos, Quaternion rotation, Vector3 scale, bool removeDefaultItems = false)
        {
            Locker = CreateLocker(lockerType, pos, rotation, scale, removeDefaultItems);
            LockerType = lockerType;

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }
        internal SynapseLockerObject(SynapseSchematic.LockerConfiguration configuration)
        {
            Locker = CreateLocker(configuration.LockerType, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale, configuration.DeleteDefaultItems);
            LockerType = configuration.LockerType;
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;
            UpdateEveryFrame = configuration.UpdateEveryFrame;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;

            for (int i = 0; i < configuration.Chambers.Count; i++)
            {
                foreach (var item in configuration.Chambers[i].Items)
                    SpawnItem(item, i);
            }
        }

        public override NetworkIdentity NetworkIdentity => Locker.locker.netIdentity;
        public override GameObject GameObject => Locker.GameObject;
        public override ObjectType Type => ObjectType.Locker;
        public override void Destroy()
        {
            Map.Get.Lockers.Remove(Locker);
            base.Destroy();
        }

        public LockerType LockerType { get; }

        public void SpawnItem(ItemType type, int chamber, int amount = 1)
        {
            if(chamber >= 0 && Locker.Chambers.Count > chamber)
            Locker.Chambers[chamber].SpawnItem(type, amount);
            UnfreezeAll();
        }

        public Locker Locker { get; }

        public Locker CreateLocker(LockerType lockerType, Vector3 pos, Quaternion rotation, Vector3 scale, bool removeDefaultItems = false)
        {
            var synapselocker = new Locker(CreateNetworkObject(Prefabs[lockerType], pos, rotation, scale));

            foreach (var chamber in synapselocker.Chambers)
                chamber.lockerChamber._spawnpoint.SetParent(synapselocker.locker.transform);

            synapselocker.locker.Start();

            foreach (var pickup in synapselocker.locker.GetComponentsInChildren<ItemPickupBase>())
            {
                if (removeDefaultItems)
                {
                    NetworkServer.Destroy(pickup.gameObject);
                }
                else
                {
                    pickup.Rb.isKinematic = false;
                    pickup.Rb.useGravity = true;
                }
            }

            return synapselocker;
        }

        private void UnfreezeAll()
        {
            foreach (Rigidbody rigidbody in SpawnablesDistributorBase.BodiesToUnfreeze)
                if (rigidbody != null)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                }
        }
    }
}
