using System;

namespace Synapse3.SynapseModule.Item;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ItemAttribute : Attribute
{
    public uint Id { get; set; }
    
    public ItemType BasedItemType { get; set; }
    
    public string Name { get; set; }

    public uint SchematicID { get; set; } = uint.MaxValue;
}