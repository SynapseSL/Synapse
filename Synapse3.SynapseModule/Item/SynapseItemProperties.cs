using System.Collections.Generic;
using Synapse3.SynapseModule.Item.SubAPI;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem
{
    private Dictionary<ItemCategory, ISubSynapseItem> _subApi = new();

    public readonly int ID;
    public readonly string Name;
    public readonly bool IsCustomItem;
    
    
    public readonly ItemType ItemType;
    public readonly ItemCategory ItemCategory;
    public ItemTierFlags TierFlags { get; }
    public ushort Serial { get; }
    public float Weight { get; }
    
    
    public override ObjectType Type => ObjectType.Item;
    public ItemState State { get; internal set; } = ItemState.BeforeSpawn;

    private bool _canBePickedUp = true;

    public bool CanBePickedUp
    {
        get => _canBePickedUp;
        set
        {
            _canBePickedUp = value;

            if (Pickup != null)
            {
                Pickup.Info.Locked = !value;
                Pickup.NetworkInfo = Pickup.Info;
                Pickup.InfoReceived(default,Pickup.Info);
            }
        }
    }
    public SchematicConfiguration SchematicConfiguration { get; set; }
    public SynapseSchematic Schematic { get; private set; }

    public float Durability
    {
        get => _subApi[ItemCategory].Durability;
        set => _subApi[ItemCategory].Durability = value;
    }
}