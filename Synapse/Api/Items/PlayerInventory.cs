using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Items
{
    public class PlayerInventory
    {
        private Player player;

        internal PlayerInventory(Player player1) => player = player1;

        public List<Items.Item> Items => player.VanillaInventory.items.Select(x => x.GetItem()).ToList();

        public void AddItem(Items.Item item)
        {
            if (item.ItemHolder == player) return;
            if (item.ItemHolder != null)
                item.Despawn();

            item.PickUp(player);
        }

        public void AddItem(ItemType type, float dur, int sight, int barrel, int other) => new Items.Item(type, dur, sight, barrel, other).PickUp(player);

        public void AddItem(int id, float dur, int sight, int barrel, int other) => new Items.Item(id, dur, sight, barrel, other).PickUp(player);



        public void RemoveItem(Items.Item item)
        {
            if (item.ItemHolder != player) return;

            item.Despawn();
        }

        public void Clear() => player.VanillaInventory.Clear();

        public void Drop(Items.Item item) => item.Drop(player.Position);

        public void DropAll() => player.VanillaInventory.ServerDropAll();
    }
}
