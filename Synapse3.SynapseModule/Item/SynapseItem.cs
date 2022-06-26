using System.Collections.Generic;
using InventorySystem;
using InventorySystem.Items;
using Neuron.Core.Logging;

namespace Synapse3.SynapseModule.Item;

public class SynapseItem
{
    public static SynapseItem None { get; } = new SynapseItem(-1);
    
    public static Dictionary<ushort, SynapseItem> AllItems { get; } = new Dictionary<ushort, SynapseItem>();

    public static SynapseItem GetSynapseItem(ushort serial)
    {
        if (!AllItems.ContainsKey(serial))
        {
            NeuronLogger.For<Synapse>().Warn("If this message appears exists a Item that is not registered. Please report this bug in our Discord as detailed as possible");
            return None;
        }
        return AllItems[serial];
    }

    public readonly int ID;

    public readonly string Name;

    public readonly bool IsCustomItem;

    public readonly ItemType ItemType;

    public readonly ItemCategory ItemCategory;

    public ItemTierFlags TierFlags { get; }

    public ushort Serial { get; }

    public float Weight { get; }
    
    private SynapseItem()
    {
        //Throwable = new ThrowableAPI(this);
    }
    public SynapseItem(int id) : this()
    {
        /*
        if (id == -1 && None == null)
        {
            ID = -1;
            ItemType = ItemType.None;
            Name = "None";
            return;
        }

        Serial = ItemSerialGenerator.GenerateNext();
        AllItems[Serial] = this;
        ID = id;
        var item = Synapse.Get<ItemService>();
        Schematic = item.GetSchematicConfiguration(ID);

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
        */
    }
}