using System;

namespace Synapse3.SynapseModule.Item;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ItemAttribute : Attribute
{
    public uint Id { get; set; }
    
    public ItemType BasedItemType { get; set; }
    
    public string Name { get; set; }

    public uint SchematicID { get; set; } = uint.MaxValue;
}