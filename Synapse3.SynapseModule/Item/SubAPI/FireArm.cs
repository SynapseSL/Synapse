using InventorySystem;
using InventorySystem.Items.Firearms;

namespace Synapse3.SynapseModule.Item.SubAPI;

public class FireArm : ISubSynapseItem
{
    private readonly SynapseItem _item;
    
    public FireArm(SynapseItem item)
    {
        _item = item;
    }
    
    public byte Ammo
    {
        get
        {
            switch (_item.State)
            {
                case ItemState.Map when _item.Pickup is FirearmPickup firearmPickup:
                    return firearmPickup.Status.Ammo;

                case ItemState.Inventory when _item.Item is Firearm firearm:
                    return firearm.Status.Ammo;
            }

            return 0;
        }
        set
        {
            switch (_item.State)
            {
                case ItemState.Map when _item.Pickup is FirearmPickup firearmPickup:
                    firearmPickup.Status = new FirearmStatus(value, firearmPickup.Status.Flags, firearmPickup.Status.Attachments);
                    break;

                case ItemState.Inventory when _item.Item is Firearm firearm:
                    firearm.Status = new FirearmStatus(value, firearm.Status.Flags, firearm.Status.Attachments);
                    break;
                
                case ItemState.BeforeSpawn:
                case ItemState.Despawned:
                    _setAmmo = true;
                    _ammo = value;
                    break;
            }
        }
    }

    public byte DefaultMaxAmmo
    {
        get
        {
            if (_item.Item != null && _item.Item is Firearm firearm)
                return firearm.AmmoManagerModule.MaxAmmo;

            if (InventoryItemLoader.AvailableItems.TryGetValue(_item.ItemType, out var firearmPrefab))
                return (firearmPrefab as Firearm)?.AmmoManagerModule.MaxAmmo ?? 0;

            return 0;
        }
    }

    public uint Attachments
    {
        get
        {
            switch (_item.State)
            {
                case ItemState.Map when _item.Pickup is FirearmPickup firearmPickup:
                    return firearmPickup.Status.Attachments;

                case ItemState.Inventory when _item.Item is Firearm firearm:
                    return firearm.Status.Attachments;
            }

            return 0;
        }
        set
        {
            switch (_item.State)
            {
                case ItemState.Map when _item.Pickup is FirearmPickup firearmPickup:
                    firearmPickup.Status = new FirearmStatus(firearmPickup.Status.Ammo, firearmPickup.Status.Flags, value);
                    break;

                case ItemState.Inventory when _item.Item is Firearm firearm:
                    firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags, value);
                    break;
                
                case ItemState.BeforeSpawn:
                case ItemState.Despawned:
                    _setAttachments = true;
                    _attachments = value;
                    break;
            }
        }
    }

    public float Durability
    {
        get => Ammo;
        set => Ammo = (byte)value;
    }

    private byte _ammo;
    private uint _attachments;
    private bool _setAmmo;
    private bool _setAttachments;

    public void ChangeState(ItemState newState)
    {
        if (_setAmmo)
        {
            Ammo = _ammo;
            _setAmmo = false;
        }

        if (_setAttachments)
        {
            Attachments = _attachments;
            _setAttachments = false;   
        }
    }
}