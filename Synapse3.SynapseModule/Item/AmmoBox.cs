using System;
using System.Collections.ObjectModel;
using System.Linq;
using InventorySystem;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Item;

public class AmmoBox
{
    private SynapsePlayer _player;

    public AmmoBox(SynapsePlayer player)
    {
        _player = player;
    }

    public ushort this[AmmoType ammo]
    {
        get
        {
            if (_player.VanillaInventory.UserInventory.ReserveAmmo.TryGetValue((ItemType)ammo, out var amount))
                return amount;

            return 0;
        }
        set
        {
            _player.VanillaInventory.UserInventory.ReserveAmmo[(ItemType)ammo] = value;
            _player.VanillaInventory.SendAmmoNextFrame = true;
        }
    }

    public ReadOnlyDictionary<AmmoType, ushort> Ammo 
        => new(_player.VanillaInventory.UserInventory.ReserveAmmo.ToDictionary(x => (AmmoType)x.Key, y => y.Value));

    public void SetAllAmmo(ushort amount)
    {
        foreach (var ammoType in (AmmoType[])Enum.GetValues(typeof(AmmoType)))
        {
            this[ammoType] = amount;
        }
    }

    public void DropAmount(AmmoType type, ushort amount)
        => _player.VanillaInventory.ServerDropAmmo((ItemType)type, amount);

    public void DropAllAmmo()
    {
        foreach (var pair in _player.VanillaInventory.UserInventory.ReserveAmmo.ToList())
        {
            _player.VanillaInventory.ServerDropAmmo(pair.Key, pair.Value);
        }
    }

    public void Clear()
    {
        _player.VanillaInventory.UserInventory.ReserveAmmo.Clear();
        _player.VanillaInventory.SendAmmoNextFrame = true;
    }
}