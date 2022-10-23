using System;
using Achievements.Handlers;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
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
    public void EquipItem(SynapsePlayer player, bool dropWhenFull = true, bool provideFully = false)
    {
        if(player.RoleType is RoleType.Spectator or RoleType.None) return;
        
        if (player.Inventory.Items.Count >= 8)
        {
            if (dropWhenFull)
                Drop(player.Position);
            return;
        }

        if (RootParent is SynapseItem parent)
        {
            parent.EquipItem(player, false);
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
        SetState(ItemState.Inventory);

        if (!provideFully || Item is not Firearm firearm) return;
        
        firearm.ApplyAttachmentsCode(player.GetPreference(ItemType), true);
        var flags = FirearmStatusFlags.MagazineInserted;
        if (firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight))
        {
            flags |= FirearmStatusFlags.FlashlightEnabled;
        }

        firearm.Status = new FirearmStatus(firearm.AmmoManagerModule.MaxAmmo, flags,
            firearm.GetCurrentAttachmentsCode());
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

        DestroyPickup();
        Throwable.DestroyProjectile();
        
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
        SetState(ItemState.Map);
        CreateSchematic();
        
        DestroyItem();

        var comp = Pickup.gameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
    }

    public override void Destroy()
    {
        DestroyItem();
        DestroyPickup();
        Throwable.DestroyProjectile();
        
        SetState(ItemState.Despawned);
    }

    internal void DestroyItem()
    {
        if(Item == null) return;
        Item.OnRemoved(Pickup);
        
        var holder = ItemOwner;
        if (holder != null)
        {
            if (holder.Inventory.ItemInHand == this)
                holder.Inventory.ItemInHand = None;
            
            holder.VanillaInventory.UserInventory.Items.Remove(Serial);
            holder.VanillaInventory.SendItemsNextFrame = true;
            holder.Inventory._items.Remove(this);
        }
        
        if(Item == null) return;
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

    internal void SetState(ItemState state)
    {
        State = state;
        _subApi[ItemCategory]?.ChangeState(state);
    }

    public override void HideFromAll() => NetworkIdentity?.UnSpawnForAllPlayers();

    public override void ShowAll() => NetworkIdentity?.SpawnForAllPlayers();

    public override void HideFromPlayer(SynapsePlayer player) => NetworkIdentity?.UnSpawnForOnePlayer(player);

    public override void ShowPlayer(SynapsePlayer player) => NetworkIdentity?.SpawnForOnePlayer(player);
}