using InventorySystem;
using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Items
{
    public class PlayerInventory
    {
        private readonly Player _player;

        internal PlayerInventory(Player player)
            => _player = player;

        public SynapseItem this[int index]
            => Items[index];

        public List<SynapseItem> Items
            => _player.VanillaInventory.UserInventory.Items.Select(x => x.Value.GetSynapseItem()).Where(x => x != null).ToList();

        public void AddItem(SynapseItem item)
            => item.PickUp(_player);

        public void AddItem(ItemType type)
            => new SynapseItem(type, _player);

        public void AddItem(int id)
            => new SynapseItem(id, _player);

        public void RemoveItem(SynapseItem item)
            => item.Destroy();

        public void Drop(SynapseItem item)
            => item.Drop(_player.Position);

        public void DropAmmo(AmmoType type, ushort amount)
            => _player.VanillaInventory.ServerDropAmmo((ItemType)type, amount);

        public void DropAll()
        {
            try
            {
                foreach (var item in Items)
                    item?.Drop();

                foreach (var ammo in _player.VanillaInventory.UserInventory.ReserveAmmo.ToList())
                    _ = _player.VanillaInventory.ServerDropAmmo(ammo.Key, ammo.Value);
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

            _player.VanillaInventory.UserInventory.ReserveAmmo.Clear();
            _player.VanillaInventory.SendAmmoNextFrame = true;
        }
    }
}
