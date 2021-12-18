using System;
using System.Collections.Generic;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Radio;
using Mirror;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.Items
{
    public class SynapseItem
    {
        public static SynapseItem None { get; } = new SynapseItem(-1);

        public static Dictionary<ushort, SynapseItem> AllItems { get; } = new Dictionary<ushort, SynapseItem>();

        private bool deactivated = false;

        #region Constructors
        private SynapseItem()
        {
            Throwable = new ThrowableAPI(this);
        }

        /// <summary>
        /// This constructor creates a completely new Item from a ItemType but wont spawn it
        /// </summary>
        /// <param name="type"></param>
        public SynapseItem(ItemType type) : this()
        {
            Serial = ItemSerialGenerator.GenerateNext();
            AllItems[Serial] = this;
            ID = (int)type;
            Name = type.ToString();
            IsCustomItem = false;
            ItemType = type;

            if (InventoryItemLoader.AvailableItems.TryGetValue(type, out var examplebase))
            {
                ItemCategory = examplebase.Category;
                TierFlags = examplebase.TierFlags;
                Weight = examplebase.Weight;
            }
        }

        /// <summary>
        /// This constructor creates a completely new Item from a ItemType and gives it a Player
        /// </summary>
        /// <param name="type"></param>
        /// <param name="player"></param>
        public SynapseItem(ItemType type, Player player) : this(type) => PickUp(player);

        /// <summary>
        /// This constructor creates a completely new Item from a ItemType and Drops it
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        public SynapseItem(ItemType type, Vector3 pos) : this(type) => Drop(pos);

        /// <summary>
        /// This constructor creates a completely new Item from a ItemID but wont spawn it
        /// </summary>
        /// <param name="type"></param>
        public SynapseItem(int id) : this()
        {
            if(id == -1 && None == null)
            {
                ID = -1;
                ItemType = ItemType.None;
                Name = "None";
                return;
            }

            Serial = ItemSerialGenerator.GenerateNext();
            AllItems[Serial] = this;
            ID = id;

            if (id >= 0 && id <= ItemManager.HighestItem)
            {
                IsCustomItem = false;
                ItemType = (ItemType)id;
                Name = ItemType.ToString();
            }
            else
            {
                IsCustomItem = true;
                ItemType = Server.Get.ItemManager.GetBaseType(id);
                Name = Server.Get.ItemManager.GetName(id);
            }

            if (InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var examplebase))
            {
                ItemCategory = examplebase.Category;
                TierFlags = examplebase.TierFlags;
                Weight = examplebase.Weight;
            }
        }

        /// <summary>
        /// This constructor creates a completely new Item from a ItemID and gives it a Player
        /// </summary>
        /// <param name="type"></param>
        /// <param name="player"></param>
        public SynapseItem(int id, Player player) : this(id) => PickUp(player);

        /// <summary>
        /// This constructor creates a completely new Item from a ItemID and Drops it
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        public SynapseItem(int id, Vector3 pos) : this(id) => Drop(pos);

        /// <summary>
        /// This Constructor should be used to register a ItemBase that is not already registered
        /// </summary>
        /// <param name="itemBase"></param>
        public SynapseItem(ItemBase itemBase) : this()
        {
            ItemBase = itemBase;
            Serial = itemBase.ItemSerial;
            AllItems[Serial] = this;
            ID = (int)itemBase.ItemTypeId;
            Name = itemBase.ItemTypeId.ToString();
            IsCustomItem = false;
            ItemType = itemBase.ItemTypeId;
            ItemCategory = itemBase.Category;
            TierFlags = itemBase.TierFlags;
            Weight = itemBase.Weight;
        }

        /// <summary>
        /// This Constructor should be used to register a ItemPickupBase that is not already registered
        /// </summary>
        /// <param name="pickupBase"></param>
        public SynapseItem(ItemPickupBase pickupBase) : this()
        {
            Serial = pickupBase.Info.Serial;
            PickupBase = pickupBase;
            AllItems[Serial] = this;
            ID = (int)pickupBase.Info.ItemId;
            Name = pickupBase.Info.ItemId.ToString();
            IsCustomItem = false;
            ItemType = pickupBase.Info.ItemId;
            if (InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var examplebase))
            {
                ItemCategory = examplebase.Category;
                TierFlags = examplebase.TierFlags;
            }
            Weight = pickupBase.Info.Weight;
        }
        #endregion

        #region API
        public ThrowableAPI Throwable { get; }
        #endregion

        #region InitialValues
        public readonly int ID;

        public readonly string Name;

        public readonly bool IsCustomItem;

        public readonly ItemType ItemType;

        public readonly ItemCategory ItemCategory;

        public ItemTierFlags TierFlags { get; }

        public ushort Serial { get; }

        public float Weight { get; }
        #endregion

        #region Gameobjects
        public GameObject ItemGameObject
        {
            get
            {
                switch (State)
                {
                    case ItemState.Map: return PickupBase.gameObject;
                    case ItemState.Inventory: return ItemBase.gameObject;
                    case ItemState.Thrown: return Throwable.ThrowableItem.gameObject;
                    default: return null;
                }
            }
        }
        public ItemBase ItemBase { get; internal set; }
        public ItemPickupBase PickupBase { get; internal set; }
        #endregion

        #region DynamicValues
        public Enum.ItemState State
        {
            get
            {
                if (deactivated) return Enum.ItemState.Destroyed;

                if (Throwable.ThrowableItem != null) return Enum.ItemState.Thrown;

                if (ItemBase != null) return Enum.ItemState.Inventory;

                if (PickupBase != null) return Enum.ItemState.Map;

                return Enum.ItemState.Despawned;
            }
        }

        public Player ItemHolder
        {
            get
            {
                switch (State)
                {
                    case ItemState.Map: return PickupBase.PreviousOwner.GetPlayer();
                    case ItemState.Inventory: return ItemBase.Owner.GetPlayer();
                    default: return null;
                }
            }
        }
        #endregion

        #region ChangableAPIValues
        public Dictionary<string, object> ItemData { get; set; } = new Dictionary<string, object>();

        private Vector3 position = Vector3.zero;
        public Vector3 Position
        {
            get
            {
                if (ItemBase != null)
                    return ItemHolder.Position;

                if (PickupBase != null)
                    return PickupBase.Info.Position;

                return position;
            }
            set
            {
                if (PickupBase != null)
                {
                    PickupBase.Rb.position = position;
                    PickupBase.RefreshPositionAndRotation();
                }

                position = value;
            }
        }

        private Quaternion rotation = default;
        public Quaternion Rotation
        {
            get
            {
                if (ItemBase != null)
                    return ItemHolder.transform.rotation;

                if (PickupBase != null)
                    return PickupBase.transform.rotation;

                return rotation;
            }
            set
            {
                if (PickupBase != null)
                {
                    PickupBase.transform.rotation = rotation;
                    PickupBase.RefreshPositionAndRotation();
                }

                rotation = value;
            }
        }

        private Vector3 scale = Vector3.one;
        public Vector3 Scale
        {
            get => scale;
            set
            {
                if (PickupBase != null)
                {
                    PickupBase.transform.localScale = value;
                    NetworkServer.UnSpawn(PickupBase.gameObject);
                    NetworkServer.Spawn(PickupBase.gameObject);
                }

                scale = value;
            }
        }

        private float durabillity = 0;
        public float Durabillity
        {
            get
            {
                switch (ItemCategory)
                {
                    case ItemCategory.Radio:
                        if (State == ItemState.Inventory)
                            return (ItemBase as RadioItem).BatteryPercent;
                        else if (State == ItemState.Map)
                            return (PickupBase as RadioPickup).SavedBattery * 100;
                        break;

                    case ItemCategory.MicroHID:
                        if (State == ItemState.Inventory)
                            return (ItemBase as MicroHIDItem).RemainingEnergy * 100;
                        else if (State == ItemState.Map)
                            return (PickupBase as MicroHIDPickup).Energy * 100;
                        break;

                    case ItemCategory.Firearm:
                        if (State == ItemState.Inventory)
                            return (ItemBase as Firearm).Status.Ammo;
                        else if (State == ItemState.Map)
                            return (PickupBase as FirearmPickup).Status.Ammo;
                        break;

                    case ItemCategory.Ammo:
                        //Ammo can't be in the inventory as Item
                        if (State == ItemState.Map)
                            return (PickupBase as AmmoPickup).SavedAmmo;
                        break;
                }

                return durabillity;
            }
            set
            {
                if(State == ItemState.Despawned)
                {
                    durabillity = value;
                    return;
                }

                switch (ItemCategory)
                {
                    case ItemCategory.Radio:
                        if (State == ItemState.Inventory)
                            (ItemBase as RadioItem)._battery = value / 100;
                        else if (State == ItemState.Map)
                            (PickupBase as RadioPickup).SavedBattery = value / 100;
                        break;

                    case ItemCategory.MicroHID:
                        if (State == ItemState.Inventory)
                            (ItemBase as MicroHIDItem).RemainingEnergy = value / 100;
                        else if (State == ItemState.Map)
                            (PickupBase as MicroHIDPickup).Energy = value / 100;
                        break;

                    case ItemCategory.Firearm:
                        if (State == ItemState.Inventory)
                        {
                            var arm = ItemBase as Firearm;
                            arm.Status = new FirearmStatus((byte)value, arm.Status.Flags, arm.Status.Attachments);
                        }
                        else if (State == ItemState.Map)
                        {
                            var armpickup = PickupBase as FirearmPickup;
                            armpickup.Status = new FirearmStatus((byte)value, armpickup.Status.Flags, armpickup.Status.Attachments);
                        }
                        break;

                    case ItemCategory.Ammo:
                        if (State == ItemState.Map)
                            (PickupBase as AmmoPickup).SavedAmmo = (ushort)value;
                        break;
                }
            }
        }

        private uint attachments = 0;
        public uint WeaponAttachments
        {
            get
            {
                if(ItemBase is Firearm arm)
                {
                    return arm.Status.Attachments;
                }
                else if (PickupBase is FirearmPickup armpickup)
                {
                    return armpickup.Status.Attachments;
                }
                return attachments;
            }
            set
            {
                if (State == ItemState.Despawned)
                {
                    attachments = value;
                    return;
                }

                if (ItemBase is Firearm arm)
                {
                    arm.ApplyAttachmentsCode(value,true);
                }
                else if (PickupBase is FirearmPickup armpickup)
                {
                    armpickup.NetworkStatus = new FirearmStatus(armpickup.Status.Ammo, armpickup.Status.Flags, value);
                }
            }
        }
        #endregion

        #region Methods
        public void PickUp(Player player)
        {
            switch (State)
            {
                case ItemState.Map:
                    player.VanillaInventory.ServerAddItem(ItemType, Serial, PickupBase);
                    PickupBase.DestroySelf();
                    break;

                case ItemState.Despawned:
                    player.VanillaInventory.ServerAddItem(ItemType, Serial);
                    Durabillity = durabillity;
                    WeaponAttachments = attachments;
                    break;

                case ItemState.Inventory:
                    DespawnItemBase();
                    player.VanillaInventory.ServerAddItem(ItemType, Serial);
                    break;
            }
        }

        public void Drop(Vector3 position)
        {
            switch (State)
            {
                case ItemState.Map: Position = position; break;

                case ItemState.Inventory:
                    ItemHolder.VanillaInventory.ServerDropItem(Serial);
                    break;

                case ItemState.Despawned:
                    if (InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var examplebase))
                    {
                        PickupBase = UnityEngine.Object.Instantiate
                            (examplebase.PickupDropModel, position, rotation);

                        var info = new PickupSyncInfo
                        {
                            Position = position,
                            Rotation = new LowPrecisionQuaternion(rotation),
                            ItemId = ItemType,
                            Serial = Serial,
                            Weight = Weight,
                        };
                        PickupBase.NetworkInfo = info;
                        PickupBase.transform.localScale = Scale;
                        NetworkServer.Spawn(PickupBase.gameObject);
                        PickupBase.InfoReceived(default, info);

                        Durabillity = durabillity;
                        WeaponAttachments = attachments;
                    }
                    break;
            }
        }

        public void Drop()
        {
            switch (State)
            {
                case ItemState.Inventory: Drop(ItemHolder.Position); break;
                case ItemState.Despawned: Drop(Position); break;
            }
        }

        public void Despawn()
        {
            durabillity = Durabillity;
            attachments = WeaponAttachments;

            DespawnItemBase();
            DespawnPickup();
            Throwable.DestroyProjectile();
        }

        public void DespawnItemBase()
        {
            if (ItemBase != null)
            {
                if(ItemHolder != null)
                {
                    ItemBase.OnRemoved(null);

                    if (ItemHolder.ItemInHand == this)
                        ItemHolder.ItemInHand = None;

                    ItemHolder.VanillaInventory.UserInventory.Items.Remove(Serial);
                    ItemHolder.VanillaInventory.SendItemsNextFrame = true;
                }

                UnityEngine.Object.Destroy(ItemBase.gameObject);
            }
        }

        public void DespawnPickup()
        {
            if (PickupBase != null) NetworkServer.Destroy(PickupBase.gameObject);
        }

        public void Destroy()
        {
            Despawn();
            AllItems.Remove(Serial);
            deactivated = true;
        }
        #endregion

        #region Obsolete
        [Obsolete("Since 11.0 only FireArms can be modified and they are using a new system", false)]
        public int Sight { get; set; }

        [Obsolete("Since 11.0 only FireArms can be modified and they are using a new system", false)]
        public int Barrel { get; set; }

        [Obsolete("Since 11.0 only FireArms can be modified and they are using a new system", false)]
        public int Other { get; set; }

        [Obsolete("Since 11.0 only FireArms can be modified and they are using a new system", false)]
        public SynapseItem(int id, float durability, int sight, int barrel, int other) : this(id) => Durabillity = durability;

        [Obsolete("Since 11.0 only FireArms can be modified and they are using a new system", false)]
        public SynapseItem(ItemType item, float durability, int sight, int barrel, int other) : this(item) => Durabillity = durability;
        #endregion
    }
}
