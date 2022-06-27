using InventorySystem;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using UnityEngine;

namespace Synapse3.SynapseModule.Item.SubAPI;

public class Throwable
{
    private SynapseItem _item;
    
    public Throwable(SynapseItem item)
    {
        _item = item;
    }
    
    /// <summary>
    /// The Projectile Object of the Item
    /// </summary>
    public ThrownProjectile Projectile { get; internal set; }
    
    /// <summary>
    /// The Time that is left until the Grenade is exploded
    /// </summary>
    public float FuseTime
    {
        get => Projectile == null ? 0 : Projectile.GetComponent<TimeGrenade>().TargetTime - Time.timeSinceLevelLoad;
        set
        {
            if (Projectile == null) return;
            var comp = Projectile.GetComponent<TimeGrenade>();

            comp.RpcSetTime(value);
            comp.UserCode_RpcSetTime(value);
        }
    }

    /// <summary>
    /// Activates the Grenade when it's already on the Map
    /// </summary>
    public void Fuse()
    {
        if(_item.State != ItemState.Map) return;
        if(!InventoryItemLoader.AvailableItems.TryGetValue(_item.ItemType, out var itemBase)) return;
        if(itemBase is not ThrowableItem throwableItem) return;

        Projectile = Object.Instantiate(throwableItem.Projectile);
        if (Projectile.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.position = _item.Pickup.Rb.position;
            rigidbody.rotation = _item.Pickup.Rb.rotation;
            rigidbody.velocity = _item.Pickup.Rb.velocity;
            rigidbody.angularVelocity = _item.Pickup.Rb.angularVelocity;
        }

        _item.Pickup.Info.Locked = true;
        Projectile.NetworkInfo = _item.Pickup.Info;
        NetworkServer.Spawn(Projectile.gameObject);
        Projectile.InfoReceived(default, _item.Pickup.Info);
        
        Projectile.ServerActivate();
        _item.DestroyPickup();
        _item.State = ItemState.Thrown;
    }

    /// <summary>
    /// Destroys the Projectile Object of the SynapseItem
    /// </summary>
    internal void DestroyProjectile()
    {
        if (Projectile is not null)
            NetworkServer.Destroy(Projectile.gameObject);
    }
}