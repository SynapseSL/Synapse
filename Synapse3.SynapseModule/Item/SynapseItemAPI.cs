using System;
using InventorySystem;
using InventorySystem.Items.Pickups;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem
{
    public void EquipItem(SynapsePlayer player)
    {
        if (player.Inventory.Items.Count >= 8)
        {
            Drop(player.Position);
            return;
        }
        
        Destroy();
        player.VanillaInventory.ServerAddItem(ItemType, Serial, Pickup);
        State = ItemState.Inventory;
        player.Inventory._items.Add(this);
    }

    public void Drop()
        => Drop(Position);

    public void Drop(Vector3 position)
    {
        if (State == ItemState.Map)
        {
            Position = position;
            return;
        }
        
        Destroy();
        
        if(!InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var exampleBase)) return;

        Pickup = UnityEngine.Object.Instantiate(exampleBase.PickupDropModel, position, _rotation);
        var info = new PickupSyncInfo
        {
            Position = position,
            Rotation = new LowPrecisionQuaternion(_rotation),
            ItemId = ItemType,
            Serial = Serial,
            Weight = Weight
        };
        Pickup.Info = info;
        Pickup.transform.localScale = Scale;
        NetworkServer.Spawn(Pickup.gameObject);
        Pickup.InfoReceived(default, info);
        UpdateSchematic();
        State = ItemState.Map;
    }

    public override void Destroy()
    {
        DestroyItem();
        DestroyPickup();
        Throwable.DestroyProjectile();
        
        State = ItemState.Despawned;
        OnDestroy();
    }

    internal void DestroyItem()
    {
        if(Item is null) return;

        var holder = ItemOwner;
        if (holder != null)
        {
            Item.OnRemoved(null);

            if (holder.Inventory.ItemInHand == this)
                holder.Inventory.ItemInHand = None;
            
            holder.VanillaInventory.UserInventory.Items.Remove(Serial);
            holder.VanillaInventory.SendItemsNextFrame = true;
            holder.Inventory._items.Remove(this);
        }
        
        Object.Destroy(Item.gameObject);
    }

    internal void DestroyPickup()
    {
        if(Pickup == null) return;

        NetworkServer.Destroy(Pickup.gameObject);
    }

    internal void UpdateSchematic()
    {
        try
        {
            if(Schematic is null || Pickup is null || SchematicConfiguration is null) return;

            Schematic = new SynapseSchematic(SchematicConfiguration);
            Schematic.Position = Position;
            Schematic.Rotation = Rotation;
            Schematic.Scale = Scale;
            Schematic.Parent = this;
            
            Pickup.netIdentity.DespawnForAllPlayers();
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>()
                .Error($"Sy3 Item: Creating schematic {SchematicConfiguration?.ID} failed for item {Name}\n" + ex);
        }
    }
}