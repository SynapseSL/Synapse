using System;

namespace Synapse3.SynapseModule.Item;

public class ItemAttribute : Attribute
{
    public int ID { get; set; }
    
    public ItemType BasedItemType { get; set; }
    
    public string Name { get; set; }

    public int SchematicID { get; set; } = -1;
}