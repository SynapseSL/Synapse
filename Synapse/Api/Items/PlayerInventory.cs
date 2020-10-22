using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Items
{
    public class PlayerInventory
    {
        private Player player;

        internal PlayerInventory(Player player1) => player = player1;

        public List<Items.SynapseItem> Items => Map.Get.Items.Where(x => x.ItemHolder == player).ToList();

        public void AddItem(Items.SynapseItem item)
        {
            if (item.ItemHolder == player) return;
            if (item.ItemHolder != null)
                item.Despawn();

            item.PickUp(player);
        }

        public void AddItem(ItemType type, float dur, int sight, int barrel, int other) => new Items.SynapseItem(type, dur, sight, barrel, other).PickUp(player);

        public void AddItem(int id, float dur, int sight, int barrel, int other) => new Items.SynapseItem(id, dur, sight, barrel, other).PickUp(player);



        public void RemoveItem(Items.SynapseItem item)
        {
            if (item.ItemHolder != player) return;

            item.Despawn();
        }

        public void Clear() => player.VanillaInventory.Clear();

        public void Drop(Items.SynapseItem item) => item.Drop(player.Position);

        public void DropAll() => player.VanillaInventory.ServerDropAll();
    }
}
