using UnityEngine;

namespace Synapse.Api.Items
{
    public class Item
    {
        private Pickup pickup;
        private Inventory.SyncItemInfo itemInfo;

        public Item(int id, float durability, int sight, int barrel, int other)
        {
            if(id >= 0 && id <= 35)
            {
                ID = id;
                IsCustomItem = false;
                ItemType = (ItemType)id;
                Name = ItemType.ToString();
            }
            else
            {
                ID = id;
                IsCustomItem = true;
                ItemType = Server.Get.ItemManager.GetBaseType(id);
                Name = Server.Get.ItemManager.GetName(id);
            }
            
            Durabillity = durability;
            Sight = sight;
            Barrel = barrel;
            Other = other;
        }

        public Item(ItemType item, float durability, int sight, int barrel, int other)
        {
            IsCustomItem = false;
            ID = (int)item;
            ItemType = item;
            Durabillity = durability;
            Sight = sight;
            Barrel = barrel;
            Other = other;
        }

        public readonly int ID;

        public readonly ItemType ItemType;

        public readonly bool IsCustomItem;

        public readonly string Name;

        public float Durabillity { get; set; }

        public int Barrel { get; set; }

        public int Sight { get; set; }

        public int Other { get; set; }

        public Player ItemHolder { get; private set; }

        public Vector3 Scale { get; set; } = Vector3.one;

        public Vector3 Position
        {
            get
            {
                if (ItemHolder != null)
                    return ItemHolder.Position;

                if (pickup != null)
                    return pickup.position;

                return Vector3.one;
            }
            set
            {
                if(pickup != null)
                {
                    pickup.Networkposition = value;
                    pickup.UpdatePosition();
                }
            }
        }

        public void PickUp(Player player)
        {
            if (ItemHolder != null) return;

            if (player.Items.Count > 8) return;

            Inventory._uniqId++;
            itemInfo = new Inventory.SyncItemInfo()
            {
                durability = Durabillity,
                id = ItemType,
                modSight = Sight,
                modBarrel = Barrel,
                modOther = Other,
                uniq = Inventory._uniqId
            };
            if (pickup != null)
                pickup.Delete();
            pickup = null;
            ItemHolder = player;
            ItemHolder.Items.Add(itemInfo);
        }

        public void Drop(Vector3 position)
        {
            if (pickup != null) return;

            pickup = Server.Get.Host.Inventory.SetPickup(ItemType, Durabillity, position, Quaternion.identity, Sight, Barrel, Other);
            if (ItemHolder != null)
                ItemHolder.Items.Remove(itemInfo);
            ItemHolder = null;
        }

        public void Drop() => Drop(Position);
    }
}
