using InventorySystem;
using InventorySystem.Items;
using Synapse3.SynapseModule.Item.SubAPI;
using Synapse3.SynapseModule.Map.Schematic;

// ReSharper disable MemberCanBePrivate.Global

namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem : DefaultSynapseObject
{
    public static SynapseItem None { get; } = new SynapseItem(-1);

    private ItemService _item;
    
    private SynapseItem()
    {
        _item = Synapse.Get<ItemService>();
        Throwable = new Throwable();
        FireArm = new FireArm();
    }
    
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
        _item.AllItems[Serial] = this;
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
}