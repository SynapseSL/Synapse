using Mirror;
using UnityEngine;

namespace Synapse.Api.Items
{
    public class SynapseItem
    {
        private bool deactivated = false;
        internal Pickup pickup;
        internal Inventory.SyncItemInfo itemInfo;

        public SynapseItem(int id, float durability, int sight, int barrel, int other)
        {
            if(id >= 0 && id <= ItemManager.HighestItem)
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

            Map.Get.Items.Add(this);
        }

        public SynapseItem(ItemType item, float durability, int sight, int barrel, int other)
        {
            IsCustomItem = false;
            ID = (int)item;
            ItemType = item;
            Name = ItemType.ToString();
            Durabillity = durability;
            Sight = sight;
            Barrel = barrel;
            Other = other;

            Map.Get.Items.Add(this);
        }

        public readonly int ID;

        public readonly ItemType ItemType;

        public readonly bool IsCustomItem;

        public readonly string Name;

        public Player ItemHolder { get; private set; }

        private float durabillity;
        public float Durabillity
        {
            get => durabillity;
            set
            {
                durabillity = value;
                Refresh();
            }
        }

        private int barrel;
        public int Barrel
        {
            get => barrel;
            set
            {
                barrel = value;
                Refresh();
            }
        }

        private int sight;
        public int Sight
        {
            get => sight;
            set
            {
                sight = value;
                Refresh();
            }
        }

        private int other;
        public int Other
        {
            get => other;
            set
            {
                other = value;
                Refresh();
            }
        }

        private Vector3 scale = Vector3.one;
        public Vector3 Scale
        {
            get => scale;
            set
            {
                scale = value;
                Refresh();
            }
        }

        private Vector3 position = Vector3.zero;
        public Vector3 Position
        {
            get
            {
                if (ItemHolder != null)
                    return ItemHolder.Position;

                if (pickup != null)
                    return pickup.position;

                return position;
            }
            set
            {
                if(pickup != null)
                    pickup.SetupPickup(pickup.itemId, pickup.durability, pickup.ownerPlayer, pickup.weaponMods, value, pickup.rotation);

                position = value;
            }
        }

        private void Refresh()
        {
            if (pickup != null)
            {
                var qua = pickup.rotation;
                var pos = Position;
                var owner = pickup.ownerPlayer;
                pickup.Delete();
                pickup = null;

                pickup = UnityEngine.Object.Instantiate(Server.Get.Host.VanillaInventory.pickupPrefab).GetComponent<Pickup>();
                pickup.transform.localScale = Scale;
                NetworkServer.Spawn(pickup.gameObject);
                pickup.SetupPickup(ItemType, Durabillity, owner, new Pickup.WeaponModifiers(true, Sight, Barrel, Other), pos, qua);
                return;
            }

            if (ItemHolder != null)
            {
                var index = ItemHolder.VanillaInventory.items.IndexOf(itemInfo);
                var item = ItemHolder.VanillaItems[index];
                item.durability = Durabillity;
                item.modSight = Sight;
                item.modBarrel = Barrel;
                item.modOther = Other;
                itemInfo = item;
                ItemHolder.VanillaItems[index] = item;
            }
        }

        public void PickUp(Player player)
        {
            if (deactivated) throw new System.Exception("Player tryied to Pickup a Destroyed Item??");

            if (ItemHolder != null) return;

            if (player.VanillaItems.Count >= 8) return;

            if(!IsCustomItem && (ItemType == ItemType.Ammo556 || ItemType == ItemType.Ammo762 || ItemType == ItemType.Ammo9mm))
            {
                switch (ItemType)
                {
                    case ItemType.Ammo556:
                        player.Ammo5 += (uint)Durabillity;
                        break;

                    case ItemType.Ammo762:
                        player.Ammo7 += (uint)Durabillity;
                        break;

                    case ItemType.Ammo9mm:
                        player.Ammo9 += (uint)Durabillity;
                        break;
                }

                Destroy();
                return;
            }

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
            ItemHolder.VanillaItems.Add(itemInfo);
        }

        public void Drop(Vector3 position)
        {
            if (deactivated) throw new System.Exception("Something tryied to Drop a Destroyed Item??");

            if (pickup != null) return;

            pickup = UnityEngine.Object.Instantiate(Server.Get.Host.VanillaInventory.pickupPrefab).GetComponent<Pickup>();
            pickup.transform.localScale = Scale;
            NetworkServer.Spawn(pickup.gameObject);
            pickup.SetupPickup(ItemType, Durabillity, ItemHolder == null ? Server.Get.Host.gameObject : ItemHolder.gameObject, new Pickup.WeaponModifiers(true, Sight, Barrel, Other), position, ItemHolder != null ? ItemHolder.VanillaInventory.camera.transform.rotation : Quaternion.identity);

            if (ItemHolder != null)
                ItemHolder.VanillaItems.Remove(itemInfo);
            ItemHolder = null;
        }

        public void Drop() => Drop(Position);

        public void Despawn()
        {
            if(pickup != null)
            {
                pickup.Delete();
                pickup = null;
            }

            if(ItemHolder != null)
            {
                ItemHolder.VanillaItems.Remove(itemInfo);
                ItemHolder = null;
            }
        }

        public void Destroy()
        {
            Despawn();
            Map.Get.Items.Remove(this);
            deactivated = true;
        }
    }
}
