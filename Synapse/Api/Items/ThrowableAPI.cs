using InventorySystem.Items.ThrowableProjectiles;
using Mirror;

namespace Synapse.Api.Items
{
    public class ThrowableAPI
    {
        private SynapseItem Item { get; }

        public ThrowableAPI(SynapseItem item) => Item = item;

        public ThrownProjectile ThrowableItem { get; internal set; }

        public void DestroyProjectile()
        {
            if (ThrowableItem != null)
                NetworkServer.Destroy(ThrowableItem.gameObject);
        }
    }
}
