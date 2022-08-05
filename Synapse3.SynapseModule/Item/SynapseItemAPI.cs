using System;
using Achievements.Handlers;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem
{
    public void EquipItem(SynapsePlayer player)
    {
        if(player.RoleType is RoleType.Spectator or RoleType.None) return;
        
        if (player.Inventory.Items.Count >= 8)
        {
            Drop(player.Position);
            return;
        }

        if (RootParent is SynapseItem parent)
        {
            parent.EquipItem(player);
            return;
        }

        DestroyItem();
        Throwable.DestroyProjectile();


        Item = player.VanillaInventory.CreateItemInstance(ItemType, player.VanillaInventory.isLocalPlayer);
        if(Item == null) return;
        
        player.VanillaInventory.UserInventory.Items[Serial] = Item;
        player.VanillaInventory.SendItemsNextFrame = true;
        player.Inventory._items.Add(this);
        
        Item.ItemSerial = Serial;
        Item.OnAdded(Pickup);
        //Normally it will call a event but we can't call it from here
        try
        {
            ItemPickupHandler.OnItemAdded(player.VanillaInventory, Item, Pickup);
        }
        catch { }

        if (player.VanillaInventory.isLocalPlayer && Item is IAcquisitionConfirmationTrigger trigger)
        {
            trigger.ServerConfirmAcqusition();
            trigger.AcquisitionAlreadyReceived = true;
        }
        
        DestroyPickup();
        State = ItemState.Inventory;
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
        
        var owner = ItemOwner;
        var rot = _rotation;

        Destroy();
        
        if(!InventoryItemLoader.AvailableItems.TryGetValue(ItemType, out var exampleBase)) return;

        if (owner is not null)
        {
            rot = owner.CameraReference.rotation * exampleBase.PickupDropModel.transform.rotation;
        }
        
        Pickup = Object.Instantiate(exampleBase.PickupDropModel, position, rot);
        var info = new PickupSyncInfo
        {
            Position = position,
            Rotation = new LowPrecisionQuaternion(rot),
            ItemId = ItemType,
            Serial = Serial,
            Weight = Weight,
            Locked = !CanBePickedUp,
        };
        Pickup.Info = info;
        Pickup.NetworkInfo = info;
        Pickup.transform.localScale = Scale;
        NetworkServer.Spawn(Pickup.gameObject);
        Pickup.InfoReceived(default, info);
        State = ItemState.Map;
        CreateSchematic();
        
        var comp = Pickup.gameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }

    public override void Destroy()
    {
        DestroyItem();
        DestroyPickup();
        Throwable.DestroyProjectile();
        
        State = ItemState.Despawned;
    }

    internal void DestroyItem()
    {
        if(Item is null) return;
        var holder = ItemOwner;
        if (holder != null)
        {
            Item.OnRemoved(Pickup);

            if (holder.Inventory.ItemInHand == this)
                holder.Inventory.ItemInHand = None;
            
            holder.VanillaInventory.UserInventory.Items.Remove(Serial);
            holder.VanillaInventory.SendItemsNextFrame = true;
            holder.Inventory._items.Remove(this);
        }

        Object.Destroy(Item.gameObject);
        Item = null;
    }

    internal void DestroyPickup()
    {
        if (Parent is SynapseSchematic schematic)
        {
            schematic._items.Remove(this);
            Parent = null;
        }
        Schematic?.Destroy();
        Schematic = null;
        if(Pickup == null) return;

        NetworkServer.Destroy(Pickup.gameObject);
        Pickup = null;
    }

    private void CreateSchematic()
    {
        try
        {
            if(Pickup == null || SchematicConfiguration == null) return;

            Schematic = new SynapseSchematic(SchematicConfiguration);
            Schematic.Position = Position;
            Schematic.Rotation = Rotation;
            Schematic.Scale = _scale;
            Schematic.Parent = this;
            Schematic.GameObject.transform.parent = Pickup.transform;

            Pickup.netIdentity.UnSpawnForAllPlayers();
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>()
                .Error($"Sy3 Item: Creating schematic {SchematicConfiguration?.Id} failed for item {Name}\n" + ex);
        }
    }
}