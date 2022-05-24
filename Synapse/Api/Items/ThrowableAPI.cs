using InventorySystem;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using UnityEngine;

namespace Synapse.Api.Items
{
    public class ThrowableAPI
    {
        private SynapseItem Item { get; }

        public ThrowableAPI(SynapseItem item) => Item = item;

        public ThrownProjectile ThrowableItem { get; internal set; }

        public float FuseTime
        {
            get => ThrowableItem is null ? 0 : ThrowableItem.GetComponent<TimeGrenade>().TargetTime - Time.timeSinceLevelLoad;
            set
            {
                if (ThrowableItem is null)
                    return;
                var comp = ThrowableItem.GetComponent<TimeGrenade>();

                comp.RpcSetTime(value);
                comp.UserCode_RpcSetTime(value);
            }
        }

        public void Fuse()
        {
            if (Item.State != Enum.ItemState.Map)
                return;

            if (!InventoryItemLoader.AvailableItems.TryGetValue(Item.ItemType, out var itemBase))
                return;

            if (!(itemBase is ThrowableItem throwableItem))
                return;

            var thrownProjectile = UnityEngine.Object.Instantiate(throwableItem.Projectile);
            if (thrownProjectile.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                rigidbody.position = Item.PickupBase.Rb.position;
                rigidbody.rotation = Item.PickupBase.Rb.rotation;
                rigidbody.velocity = Item.PickupBase.Rb.velocity;
                rigidbody.angularVelocity = rigidbody.angularVelocity;
            }

            Item.PickupBase.Info.Locked = true;
            thrownProjectile.NetworkInfo = Item.PickupBase.Info;
            NetworkServer.Spawn(thrownProjectile.gameObject);
            thrownProjectile.InfoReceived(default, Item.PickupBase.Info);

            ThrowableItem = thrownProjectile;

            thrownProjectile.ServerActivate();
            Item.DespawnPickup();
        }

        public void DestroyProjectile()
        {
            if (ThrowableItem != null)
                NetworkServer.Destroy(ThrowableItem.gameObject);
        }
    }
}
