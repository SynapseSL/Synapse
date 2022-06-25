using System;
using Synapse3.SynapseModule.Enums;

namespace Synapse3.SynapseModule.Player;

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

    public void SetAllAmmo(ushort amount)
    {
        foreach (var ammoType in (AmmoType[])Enum.GetValues(typeof(AmmoType)))
        {
            this[ammoType] = amount;
        }
    }
}