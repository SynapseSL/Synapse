using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Item;

public abstract class CustomItemHandler : InjectedLoggerBase
{
    private readonly ItemEvents _items;
    private readonly PlayerEvents _player;

    public CustomItemHandler(ItemEvents items, PlayerEvents player)
    {
        _items = items;
        _player = player;
    }

    public virtual void Load() => HookEvents();

    public virtual void HookEvents()
    {
        _items.ReloadWeapon.Subscribe(Reload);
        _items.Shoot.Subscribe(Shoot);
        _items.ConsumeItem.Subscribe(Consume);
        _items.ThrowGrenade.Subscribe(Throw);
        _player.Pickup.Subscribe(Pickup);
        _player.DropItem.Subscribe(Drop);
        _player.ChangeItem.Subscribe(SwitchHoldItem);
    }

    public virtual void UnHookEvents()
    {
        _items.ReloadWeapon.Unsubscribe(Reload);
        _items.Shoot.Unsubscribe(Shoot);
        _items.ConsumeItem.Unsubscribe(Consume);
        _items.ThrowGrenade.Unsubscribe(Throw);
        _player.Pickup.Unsubscribe(Pickup);
        _player.DropItem.Unsubscribe(Drop);
        _player.ChangeItem.Unsubscribe(SwitchHoldItem);
    }

    public virtual void OnReload(ReloadWeaponEvent ev)
    {
        if (VanillaReload) return;
        ev.Allow = false;
        
        if(!Reloadable || ev.Item.Durability >= MagazineSize) return;

        switch (AmmoType)
        {
            case 19:
            case 22:
            case 27:
            case 28:
            case 29:
                var amountToReload = MagazineSize - ev.Item.Durability;

                if (ev.Player.Inventory.AmmoBox[(AmmoType)AmmoType] < amountToReload * NeededForOneShot)
                    amountToReload = ev.Player.Inventory.AmmoBox[(AmmoType)AmmoType] / NeededForOneShot;

                if (amountToReload <= 0) return;
                
                ev.Item.Durability += amountToReload;
                ev.Player.Inventory.AmmoBox[(AmmoType)AmmoType] -= (ushort)(amountToReload * NeededForOneShot);

                ev.PlayAnimationOverride = true;
                break;
            
            default:
                var amount = 0;
                var itemsToDelete = new List<SynapseItem>();
                foreach (var item in ev.Player.Inventory.Items)
                {
                    if(item.Id != AmmoType) continue;
                    
                    if(ev.Item.Durability >= MagazineSize) break;
                    
                    amount++;
                    itemsToDelete.Add(item);
                    if (amount < NeededForOneShot) continue;
                    
                    ev.Item.Durability++;

                    foreach (var synapseItem in itemsToDelete)
                    {
                        synapseItem.Destroy();
                    }
                    amount = 0;
                    itemsToDelete.Clear();
                    ev.PlayAnimationOverride = true;
                }
                break;
        }
    }
    
    public virtual void OnConsume(ConsumeItemEvent ev) { }
    
    public virtual void OnPickup(PickupEvent ev) { }
    
    public virtual void OnDrop(DropItemEvent ev) { }
    
    public virtual void OnEquip(ChangeItemEvent ev) { }
    
    public virtual void OnUnEquip(ChangeItemEvent ev) { }
    
    public virtual void OnShoot(ShootEvent ev) { }
    
    public virtual void OnThrow(ThrowGrenadeEvent ev) { }


    private void SwitchHoldItem(ChangeItemEvent ev)
    {
        if (ev.NewItem?.Id == Attribute.Id) OnEquip(ev);
        if (ev.PreviousItem?.Id == Attribute.Id) OnUnEquip(ev);
    }

    private void Pickup(PickupEvent ev)
    {
        if(ev.Item?.Id != Attribute.Id) return;
        OnPickup(ev);
    }
    private void Drop(DropItemEvent ev)
    {
        if(ev.ItemToDrop?.Id != Attribute.Id) return;
        OnDrop(ev);
    }
    private void Consume(ConsumeItemEvent ev)
    {
        if(ev.Item?.Id != Attribute.Id) return;
        OnConsume(ev);
    }
    private void Reload(ReloadWeaponEvent ev)
    {
        if (ev.Item?.Id != Attribute.Id) return;
        OnReload(ev);
    }
    private void Shoot(ShootEvent ev)
    {
        if(ev.Item?.Id != Attribute.Id) return;
        OnShoot(ev);
    }

    private void Throw(ThrowGrenadeEvent ev)
    {
        if(ev.Item?.Id != Attribute.Id) return;
        OnThrow(ev);
    }
    
    public ItemAttribute Attribute { get; set; }

    public virtual bool VanillaReload => true;
    public virtual bool Reloadable => true;
    public virtual int MagazineSize => 40;
    public virtual int NeededForOneShot => 1;
    public virtual uint AmmoType => (uint)ItemType.Coin;
}