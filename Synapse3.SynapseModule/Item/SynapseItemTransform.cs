using UnityEngine;

namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem
{
    private Vector3 _position = Vector3.zero;
    public override Vector3 Position
    {
        get
        {
            if (Throwable.Projectile != null)
                return Throwable.Projectile.transform.position;

            if (Pickup != null)
                return Pickup.Info.Position;
            
            if (Item != null)
                return ItemOwner?.Position ?? _position;

            return _position;
        }
        set
        {
            _position = value;

            if (Pickup != null)
            {
                Pickup.Rb.position = value;
                Pickup.RefreshPositionAndRotation();
            }

            if (Throwable.Projectile != null)
            {
                Throwable.Projectile.transform.position = value;
                Throwable.Projectile.RefreshPositionAndRotation();
            }
        }
    }

    private Quaternion _rotation = Quaternion.identity;

    public override Quaternion Rotation
    {
        get
        {
            if (Throwable.Projectile is not null)
                return Throwable.Projectile.transform.rotation;

            if (Item is not null)
                return ItemOwner?.Rotation ?? _rotation;

            if (Pickup is not null)
                return Pickup.Info.Rotation;

            return _rotation;
        }
        set
        {
            _rotation = value;

            if (Pickup is not null)
            {
                Pickup.Rb.rotation = value;
                Pickup.RefreshPositionAndRotation();
            }

            if (Throwable.Projectile is not null)
            {
                Throwable.Projectile.transform.rotation = value;
                Throwable.Projectile.RefreshPositionAndRotation();
            }
        }
    }

    private Vector3 _scale = Vector3.one;

    public override Vector3 Scale
    {
        get
        {
            if (Schematic != null)
                return Schematic.Scale;

            return _scale;
        }
        set
        {
            _scale = value;
            if (Schematic is not null)
            {
                Schematic.Scale = value;
                return;
            }

            if (Pickup is not null)
            {
                Pickup.transform.localScale = value;
                Pickup.netIdentity.UpdatePositionRotationScale();
            }
        }
    }
}