using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InventorySystem.Items;
using Synapse3.SynapseModule.Config;
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
    public ReadOnlyCollection<SynapseItem> Items => _items.ToList().AsReadOnly();
    
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
            if (!ItemInHand.Item.CanHolster()) return;
            
            if (value == null || value == SynapseItem.None || !_items.Contains(value))
            {
                _player.VanillaInventory.NetworkCurItem = ItemIdentifier.None;
                _player.VanillaInventory.CurInstance = null;
                return;
            }
            
            if(!value.Item.CanEquip()) return;

            _player.VanillaInventory.NetworkCurItem = new ItemIdentifier(value.ItemType, value.Serial);
            _player.VanillaInventory.CurInstance = value.Item;
        }
    }

    public SerializedPlayerInventory Serialized
    {
        get => new (_player);
        set => value.Apply(_player);
    }
    
    public AmmoBox AmmoBox { get; }

    public void GiveItem(SynapseItem item) => item.EquipItem(_player);
    public SynapseItem GiveItem(uint id) => new(id, _player);
    public SynapseItem GiveItem(ItemType itemType) => new(itemType, _player);

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

    public void Clear()
    {
        ClearAllItems();
        AmmoBox.Clear();
    }

    public void ClearAllItems()
    {
        foreach (var item in Items)
        {
            item.Destroy();
        }
    }
} 