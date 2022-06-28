﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using InventorySystem.Items;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Item;

public class ItemInventory
{
    private readonly SynapsePlayer _player;

    public ItemInventory(SynapsePlayer player)
    {
        _player = player;
        AmmoBox = new AmmoBox(player);
    }
    
    internal List<SynapseItem> _items = new();
    public ReadOnlyCollection<SynapseItem> Items => _items.AsReadOnly();
    
    public SynapseItem ItemInHand
    {
        get
        {
            if (_player.VanillaInventory.CurItem == ItemIdentifier.None || _player.VanillaInventory.CurInstance == null)
                return SynapseItem.None;

            return Synapse.Get<ItemService>().GetSynapseItem(_player.VanillaInventory.CurItem.SerialNumber);
        }
        set
        {
            if (value == null || value == SynapseItem.None) //TODO:|| !Inventory.Items.Contains(value))
            {
                _player.VanillaInventory.NetworkCurItem = ItemIdentifier.None;
                _player.VanillaInventory.CurInstance = null;
                return;
            }
            
            if(!ItemInHand.Item.CanHolster() || !value.Item.CanEquip()) return;

            _player.VanillaInventory.NetworkCurItem = new ItemIdentifier(value.ItemType, value.Serial);
            _player.VanillaInventory.CurInstance = value.Item;
        }
    }
    
    public AmmoBox AmmoBox { get; }

    public void GiveItem(SynapseItem item) => item.EquipItem(_player);
    public void GiveItem(int id) => new SynapseItem(id, _player);
    public void GiveItem(ItemType itemType) => new SynapseItem(itemType, _player);

    public void RemoveItem(SynapseItem item) => item.Destroy();

    public void Drop(SynapseItem item) => item.Drop(_player.Position);
    public void DropAllItems()
    {
        foreach (var item in Items)
        {
            item.Drop(_player.Position);
        }
    }
    public void DropEverything()
    {
        DropAllItems();
        AmmoBox.DropAllAmmo();
    }

    public void ClearAllItems()
    {
        foreach (var item in Items)
        {
            item.Destroy();
        }
    }
} 