using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Synapse3.SynapseModule.Player;
using UnityEngine;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem
{
    public override GameObject GameObject
    {
        get
        {
            switch (State)
            {
                case ItemState.Map: return Pickup.gameObject;
                case ItemState.Inventory: return Item.gameObject;
                case ItemState.Thrown: return Throwable.Projectile.gameObject;
                
                case ItemState.BeforeSpawn:
                case ItemState.Despawned:
                default: return null;
            }
        }
    }
    public ItemBase Item { get; internal set; }
    public ItemPickupBase Pickup { get; internal set; }

    public SynapsePlayer ItemOwner => Item?.Owner;
}