using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Scp914;
using Synapse3.SynapseModule.Item.SubAPI;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Map.Scp914;
using Synapse3.SynapseModule.Player;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem : DefaultSynapseObject
{
    /// <summary>
    /// The Default SynapseItem that will be returned instead of null
    /// </summary>
    public static SynapseItem None { get; } = new(uint.MaxValue);

    private readonly ItemService _item;
    
    /// <summary>
    /// Private constructor that will be called by all other constructors
    /// </summary>
    private SynapseItem()
    {
        _item = Synapse.Get<ItemService>();
        Throwable = new Throwable(this);
        FireArm = new FireArm(this);

        _subApi[ItemCategory.Grenade] = Throwable;
        _subApi[ItemCategory.Firearm] = FireArm;
        _subApi[ItemCategory.Ammo] = null;
        _subApi[ItemCategory.Armor] = null;
        _subApi[ItemCategory.Keycard] = null;
        _subApi[ItemCategory.Medical] = null;
        _subApi[ItemCategory.None] = null;
        _subApi[ItemCategory.Radio] = null;
        _subApi[ItemCategory.MicroHID] = null;
        _subApi[ItemCategory.SCPItem] = null;

        MoveInElevator = true;
    }
    
    /// <summary>
    /// Creates a new SynapseItem based on an given ID
    /// </summary>
    public SynapseItem(uint id) : this()
    {
        if (id == uint.MaxValue && None == null)
        {
            Id = uint.MaxValue;
            ItemType = ItemType.None;
            Name = "None";
            return;
        }

        Serial = ItemSerialGenerator.GenerateNext();
        _item._allItems[Serial] = this;
        Id = id;

        SchematicConfiguration = _item.GetSchematicConfiguration(Id);

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

        var processor = Synapse.Get<Scp914Service>().GetProcessor(Id);
        if (processor == Default914Processor.DefaultProcessor)
        {
            if (Scp914Upgrader.TryGetProcessor(ItemType, out var vanillaProcessor))
                processor = new Default914Processor(vanillaProcessor);
        }

        UpgradeProcessor = processor;
    }

    /// <summary>
    /// Creates a new SynapseItem based on an given ID and adds it to the player's inventory
    /// </summary>
    public SynapseItem(uint id, SynapsePlayer player) : this(id)
        => EquipItem(player);

    /// <summary>
    /// Creates a new SynapseItem based on an given ID and spawns it on the Map
    /// </summary>
    public SynapseItem(uint id, Vector3 position) : this(id)
        => Drop(position);
    
    /// <summary>
    /// Creates a new SynapseItem based on an given ItemType
    /// </summary>
    public SynapseItem(ItemType item) : this((uint)item) { }

    /// <summary>
    /// Creates a new SynapseItem based on an given ItemType and adds it to the player's inventory
    /// </summary>
    public SynapseItem(ItemType item, SynapsePlayer player) : this((uint)item, player) { }

    /// <summary>
    /// Creates a new SynapseItem based on an given ItemType and spawns it on the Map
    /// </summary>
    public SynapseItem(ItemType item, Vector3 position) : this((uint)item, position) { }


    /// <summary>
    /// Creates a new SynapseItem with an already existing but not yet registered ItemBase
    /// </summary>
    public SynapseItem(ItemBase itemBase) : this()
    {
        Item = itemBase;
        Serial = itemBase.ItemSerial;
        _item._allItems[Serial] = this;
        Id = (uint)itemBase.ItemTypeId;
        SchematicConfiguration = _item.GetSchematicConfiguration(Id);
        Name = itemBase.ItemTypeId.ToString();
        IsCustomItem = false;
        ItemType = itemBase.ItemTypeId;
        ItemCategory = itemBase.Category;
        TierFlags = itemBase.TierFlags;
        Weight = itemBase.Weight;

        State = ItemState.Inventory;
        
        var processor = Synapse.Get<Scp914Service>().GetProcessor(Id);
        if (processor == Default914Processor.DefaultProcessor)
        {
            if (Scp914Upgrader.TryGetProcessor(ItemType, out var vanillaProcessor))
                processor = new Default914Processor(vanillaProcessor);
        }

        UpgradeProcessor = processor;
    }

    /// <summary>
    /// Creates a new SynapseItem with an already existing but not yet registered ItemPickup
    /// </summary>
    public SynapseItem(ItemPickupBase pickupBase) : this()
    {
        Serial = pickupBase.Info.Serial;
        Pickup = pickupBase;
        _item._allItems[Serial] = this;
        Id = (uint)pickupBase.Info.ItemId;
        SchematicConfiguration = _item.GetSchematicConfiguration(Id);
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

        var processor = Synapse.Get<Scp914Service>().GetProcessor(Id);
        if (processor == Default914Processor.DefaultProcessor)
        {
            if (Scp914Upgrader.TryGetProcessor(ItemType, out var vanillaProcessor))
                processor = new Default914Processor(vanillaProcessor);
        }

        UpgradeProcessor = processor;
    }

    internal SynapseItem(SchematicConfiguration.ItemConfiguration configuration, SynapseSchematic schematic) : this()
    {
        Serial = ItemSerialGenerator.GenerateNext();
        _item._allItems[Serial] = this;
        Id = (uint)configuration.ItemType;
        
        if (Id is >= 0 and <= ItemService.HighestItem)
        {
            IsCustomItem = false;
            ItemType = (ItemType)Id;
            Name = ItemType.ToString();
        }
        else
        {
            IsCustomItem = true;
            ItemType = _item.GetBaseType(Id);
            Name = _item.GetName(Id);
        }
        
        if (InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var exampleBase))
        {
            ItemCategory = exampleBase.Category;
            TierFlags = exampleBase.TierFlags;
            Weight = exampleBase.Weight;
        }
        
        var processor = Synapse.Get<Scp914Service>().GetProcessor(Id);
        if (processor == Default914Processor.DefaultProcessor)
        {
            if (Scp914Upgrader.TryGetProcessor(ItemType, out var vanillaProcessor))
                processor = new Default914Processor(vanillaProcessor);
        }
        
        UpgradeProcessor = processor;

        Drop(configuration.Position);
        Rotation = configuration.Rotation;
        Scale = configuration.Scale;
        CanBePickedUp = configuration.CanBePickedUp;
        
        if (!configuration.Physics)
        {
            Pickup.Rb.useGravity = false;
            Pickup.Rb.isKinematic = true;
        }
        
        Parent = schematic;
        Pickup.transform.parent = schematic.GameObject.transform;
        schematic._items.Add(this);
        
        OriginalScale = configuration.Scale;
        CustomAttributes = configuration.CustomAttributes;
        
        Durability = configuration.Durabillity;
        FireArm.Attachments = configuration.Attachments;
    }
}