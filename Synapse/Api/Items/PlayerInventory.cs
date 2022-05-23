using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using Synapse.Api.Enum;

namespace Synapse.Api.Items
{
    public class PlayerInventory
    {
        private readonly Player player;

        internal PlayerInventory(Player player1) => player = player1;

        public SynapseItem this[int index]
        {
            get => Items[index];
        }

        public List<Items.SynapseItem> Items => player.VanillaInventory.UserInventory.Items.Select(x => x.Value.GetSynapseItem()).Where(x => x != null).ToList();

        public void AddItem(SynapseItem item) => item.PickUp(player);

        public void AddItem(ItemType type) => new SynapseItem(type, player);

        public void AddItem(int id) => new SynapseItem(id, player);

        public void RemoveItem(SynapseItem item) => item.Destroy();

        public void Drop(SynapseItem item) => item.Drop(player.Position);

        public void DropAmmo(AmmoType type, ushort amount) => player.VanillaInventory.ServerDropAmmo((ItemType)type, amount);

        public void DropAll()
        {
            try
            {
                foreach (var item in Items)
                    item?.Drop();

                foreach (var ammo in player.VanillaInventory.UserInventory.ReserveAmmo.ToList())
                    player.VanillaInventory.ServerDropAmmo(ammo.Key, ammo.Value);
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Error while Dropping all Items:\n{e}");
            }
        }

        public void Clear()
        {
            foreach (var item in Items)
                item.Destroy();

            player.VanillaInventory.UserInventory.ReserveAmmo.Clear();
            player.VanillaInventory.SendAmmoNextFrame = true;
        }
    }
}