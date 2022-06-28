using System.Net.NetworkInformation;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Synapse3.SynapseModule.Item.SubAPI;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem : DefaultSynapseObject
{
    /// <summary>
    /// The Default SynapseItem that will be returned instead of null
    /// </summary>
    public static SynapseItem None { get; } = new SynapseItem(-1);

    private ItemService _item;
    
    /// <summary>
    /// Private constructor that will be called by all other constructors
    /// </summary>
    private SynapseItem()
    {
        _item = Synapse.Get<ItemService>();
        Throwable = new Throwable(this);
        FireArm = new FireArm();
    }
    
    /// <summary>
    /// Creates a new SynapseItem based on an given ID
    /// </summary>
    public SynapseItem(int id) : this()
    {
        if (id == -1 && None == null)
        {
            ID = -1;
            ItemType = ItemType.None;
            Name = "None";
            return;
        }

        Serial = ItemSerialGenerator.GenerateNext();
        _item._allItems[Serial] = this;
        ID = id;

        SchematicConfiguration = _item.GetSchematicConfiguration(ID);

        if (id is >= 0 and <= ItemService.HighestItem)
        {
            IsCustomItem = false;
            ItemType = (ItemType)id;
            Name = ItemType.ToString();
        }
        else
        {
            IsCustomItem = true;
            ItemType = _item.GetBaseType(id);
            Name = _item.GetName(id);
        }

        if (InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var exampleBase))
        {
            ItemCategory = exampleBase.Category;
            TierFlags = exampleBase.TierFlags;
            Weight = exampleBase.Weight;
        }
    }

    /// <summary>
    /// Creates a new SynapseItem based on an given ID and adds it to the player's inventory
    /// </summary>
    public SynapseItem(int id, SynapsePlayer player) : this(id)
        => EquipItem(player);

    /// <summary>
    /// Creates a new SynapseItem based on an given ID and spawns it on the Map
    /// </summary>
    public SynapseItem(int id, Vector3 position) : this(id)
        => Drop(position);
    
    /// <summary>
    /// Creates a new SynapseItem based on an given ItemType
    /// </summary>
    public SynapseItem(ItemType role) : this((int)role) { }
    
    /// <summary>
    /// Creates a new SynapseItem based on an given ItemType and adds it to the player's inventory
    /// </summary>
    public SynapseItem(ItemType role, SynapsePlayer player) : this((int)role, player) { }
    
    /// <summary>
    /// Creates a new SynapseItem based on an given ItemType and spawns it on the Map
    /// </summary>
    public SynapseItem(ItemType role, Vector3 position) : this((int)role,position) { }


    /// <summary>
    /// Creates a new SynapseItem with an already existing but not yet registered ItemBase
    /// </summary>
    public SynapseItem(ItemBase itemBase) : this()
    {
        Item = itemBase;
        Serial = itemBase.ItemSerial;
        _item._allItems[Serial] = this;
        ID = (int)itemBase.ItemTypeId;
        SchematicConfiguration = _item.GetSchematicConfiguration(ID);
        Name = itemBase.ItemTypeId.ToString();
        IsCustomItem = false;
        ItemType = itemBase.ItemTypeId;
        ItemCategory = itemBase.Category;
        TierFlags = itemBase.TierFlags;
        Weight = itemBase.Weight;

        State = ItemState.Inventory;
    }

    /// <summary>
    /// Creates a new SynapseItem with an already existing but not yet registered ItemPickup
    /// </summary>
    public SynapseItem(ItemPickupBase pickupBase) : this()
    {
        Serial = pickupBase.Info.Serial;
        Pickup = pickupBase;
        _item._allItems[Serial] = this;
        ID = (int)pickupBase.Info.ItemId;
        SchematicConfiguration = _item.GetSchematicConfiguration(ID);
        Name = pickupBase.Info.ItemId.ToString();
        IsCustomItem = false;
        ItemType = pickupBase.Info.ItemId;
        if (InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var exampleBase))
        {
            ItemCategory = exampleBase.Category;
            TierFlags = exampleBase.TierFlags;
        }
        Weight = pickupBase.Info.Weight;

        State = ItemState.Map;
    }

    internal SynapseItem(SchematicConfiguration.ItemConfiguration configuration, SynapseSchematic schematic) : this(configuration.ItemType)
    {
        Parent = schematic;
        schematic._items.Add(this);
        
        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
        
        Drop(configuration.Position);
        Rotation = configuration.Rotation;
        Scale = configuration.Scale;
        CanBePickedUp = configuration.CanBePickedUp;
        if (!configuration.Physics)
        {
            Pickup.Rb.useGravity = false;
            Pickup.Rb.isKinematic = true;
        }
        //TODO: Durability
    }
}