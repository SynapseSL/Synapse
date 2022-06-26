using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem
{
    public readonly int ID;
    public readonly string Name;
    public readonly bool IsCustomItem;
    
    
    public readonly ItemType ItemType;
    public readonly ItemCategory ItemCategory;
    public ItemTierFlags TierFlags { get; }
    public ushort Serial { get; }
    public float Weight { get; }
    
    
    public override ObjectType Type => ObjectType.Item;
    public ItemState State { get; private set; } = ItemState.BeforeSpawn;

    public bool CanBePickedUp { get; set; } = false;
    public SchematicConfiguration SchematicConfiguration { get; set; }
    public SynapseSchematic Schematic { get; private set; }
}