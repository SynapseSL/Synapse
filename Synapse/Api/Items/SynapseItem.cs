﻿using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Radio;
using Mirror;
using Synapse.Api.CustomObjects;
using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.Items
{
    public class SynapseItem
    {
        public static SynapseItem None { get; } = new(-1);

        public static Dictionary<ushort, SynapseItem> AllItems { get; } = new();

        public static SynapseItem GetSynapseItem(ushort serial)
        {
            if (!AllItems.ContainsKey(serial))
            {
                Logger.Get.Warn("If this message appears exists a Item that is not registered. Please report this bug in our Discord as detailed as possible");
                return None;
            }
            return AllItems[serial];
        }

        private bool deactivated = false;

        #region Constructors
        private SynapseItem()
        {
            Throwable = new(this);
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
            Schematic = ItemManager.Get.GetSchematic(ID);
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
            if (id == -1 && None is null)
            {
                ID = -1;
                ItemType = ItemType.None;
                Name = "None";
                return;
            }

            Serial = ItemSerialGenerator.GenerateNext();
            AllItems[Serial] = this;
            ID = id;
            Schematic = ItemManager.Get.GetSchematic(ID);

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
            Schematic = ItemManager.Get.GetSchematic(ID);
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
            Schematic = ItemManager.Get.GetSchematic(ID);
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
                return State switch
                {
                    ItemState.Map => PickupBase.gameObject,
                    ItemState.Inventory => ItemBase.gameObject,
                    ItemState.Thrown => Throwable.ThrowableItem.gameObject,
                    _ => null,
                };
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

                if (Throwable.ThrowableItem is not null) return Enum.ItemState.Thrown;

                if (ItemBase is not null) return Enum.ItemState.Inventory;

                if (PickupBase is not null) return Enum.ItemState.Map;

                return Enum.ItemState.Despawned;
            }
        }

        public Player ItemHolder
        {
            get
            {
                return State switch
                {
                    //case ItemState.Map: return PickupBase.PreviousOwner.Hub.GetPlayer();
                    ItemState.Inventory => ItemBase.Owner.GetPlayer(),
                    _ => null,
                };
            }
        }
        #endregion

        #region ChangableAPIValues
        public Dictionary<string, object> ItemData { get; set; } = new();

        public bool CanBePickedUp { get; set; } = true;
        public SynapseSchematic Schematic { get; set; }
        public SynapseObject SynapseObject { get; set; }

        private Vector3 position = Vector3.zero;
        public Vector3 Position
        {
            get
            {
                if (Throwable.ThrowableItem is not null)
                    return Throwable.ThrowableItem.transform.position;

                if (ItemBase is not null)
                    return ItemHolder.Position;

                if (PickupBase is not null)
                    return PickupBase.Info.Position;

                return position;
            }
            set
            {
                if (PickupBase is not null)
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
                if (ItemBase is not null)
                    return ItemHolder.transform.rotation;

                if (PickupBase is not null)
                    return PickupBase.transform.rotation;

                return rotation;
            }
            set
            {
                if (PickupBase is not null)
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
                if (PickupBase is not null)
                {
                    PickupBase.transform.localScale = value;

                    if (SynapseObject is null)
                        PickupBase.netIdentity.UpdatePositionRotationScale();
                    else
                        SynapseObject.Scale = SynapseObject.GameObject.transform.lossyScale;
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
                        if (State is ItemState.Inventory)
                            return (ItemBase as RadioItem).BatteryPercent;
                        else if (State is ItemState.Map)
                            return (PickupBase as RadioPickup).SavedBattery * 100;
                        break;

                    case ItemCategory.MicroHID:
                        if (State is ItemState.Inventory)
                            return (ItemBase as MicroHIDItem).RemainingEnergy * 100;
                        else if (State is ItemState.Map)
                            return (PickupBase as MicroHIDPickup).Energy * 100;
                        break;

                    case ItemCategory.Firearm:
                        if (State is ItemState.Inventory)
                            return (ItemBase as Firearm).Status.Ammo;
                        else if (State is ItemState.Map)
                            return (PickupBase as FirearmPickup).Status.Ammo;
                        break;

                    case ItemCategory.Ammo:
                        //Ammo can't be in the inventory as Item
                        if (State is ItemState.Map)
                            return (PickupBase as AmmoPickup).SavedAmmo;
                        break;
                }

                return durabillity;
            }
            set
            {
                if (State is ItemState.Despawned)
                {
                    durabillity = value;
                    return;
                }

                switch (ItemCategory)
                {
                    case ItemCategory.Radio:
                        if (State is ItemState.Inventory)
                            (ItemBase as RadioItem)._battery = value / 100;
                        else if (State is ItemState.Map)
                            (PickupBase as RadioPickup).SavedBattery = value / 100;
                        break;

                    case ItemCategory.MicroHID:
                        if (State is ItemState.Inventory)
                            (ItemBase as MicroHIDItem).RemainingEnergy = value / 100;
                        else if (State is ItemState.Map)
                            (PickupBase as MicroHIDPickup).Energy = value / 100;
                        break;

                    case ItemCategory.Firearm:
                        if (State is ItemState.Inventory)
                        {
                            var arm = ItemBase as Firearm;
                            arm.Status = new FirearmStatus((byte)value, arm.Status.Flags, arm.Status.Attachments);
                        }
                        else if (State is ItemState.Map)
                        {
                            var armpickup = PickupBase as FirearmPickup;
                            armpickup.Status = new FirearmStatus((byte)value, armpickup.Status.Flags, armpickup.Status.Attachments);
                        }
                        break;

                    case ItemCategory.Ammo:
                        if (State is ItemState.Map)
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
                if (ItemBase is Firearm arm)
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
                if (State is ItemState.Despawned)
                {
                    attachments = value;
                    return;
                }

                if (ItemBase is Firearm arm)
                    arm.ApplyAttachmentsCode(value, true);
                else if (PickupBase is FirearmPickup armpickup)
                    armpickup.NetworkStatus = new FirearmStatus(armpickup.Status.Ammo, armpickup.Status.Flags, value);

            }
        }
        #endregion

        #region Methods
        public void PickUp(Player player)
        {
            if (player.Inventory.Items.Count >= 8)
            {
                Drop(player.Position);
                return;
            }

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
                        PickupBase.Info = info;
                        PickupBase.transform.localScale = Scale;
                        NetworkServer.Spawn(PickupBase.gameObject);
                        PickupBase.InfoReceived(default, info);

                        Durabillity = durabillity;
                        WeaponAttachments = attachments;

                        CheckForSchematic();
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
            if (ItemBase is not null)
            {
                if (ItemHolder is not null)
                {
                    ItemBase.OnRemoved(null);

                    if (ItemHolder.ItemInHand == this)
                        ItemHolder.ItemInHand = None;

                    ItemHolder.VanillaInventory.UserInventory.Items.Remove(Serial);
                    ItemHolder.VanillaInventory.SendItemsNextFrame = true;
                }

                UnityEngine.Object.Destroy(ItemBase.gameObject);
                ItemBase = null;
            }
        }

        public void DespawnPickup()
        {
            if (PickupBase != null) NetworkServer.Destroy(PickupBase.gameObject);
            PickupBase = null;
        }

        public void Destroy()
        {
            Despawn();
            AllItems.Remove(Serial);
            deactivated = true;
        }

        internal void CheckForSchematic()
        {
            try
            {
                if (Schematic is null) return;
                if (PickupBase is null) return;

                SynapseObject = new(Schematic)
                {
                    Position = Position,
                    ItemParent = this
                };

                var scale = Scale;
                Scale = Vector3.one;
                SynapseObject.GameObject.transform.parent = PickupBase.transform;
                Scale = scale;
                SynapseObject.Scale = SynapseObject.GameObject.transform.lossyScale;

                PickupBase?.netIdentity.DespawnForAllPlayers();
            }
            catch (Exception ex)
            {
                Logger.Get.Error($"Synapse-Item: Creating the Schematic {Schematic?.ID} for a Item failed\n" + ex);
            }
        }
        #endregion
    }
}