using Mirror;
using PlayerRoles.FirstPersonControl;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// The current Position of the Player
    /// </summary>
    public Vector3 Position
    {
        get => (CurrentRole as IFpcRole)?.FpcModule?.Position ?? Vector3.zero;
        //TODO:
        set => Hub.TryOverridePosition(value, new Vector3(0f, 0f, 0f));
    }
    
    /// <summary>
    /// The Rotation of the Player as Quaternion
    /// </summary>
    public virtual Quaternion Rotation
    {
        get => Quaternion.Euler(CameraReference.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
        set
        {
            //TODO:
            var euler = value.eulerAngles;
            //PlayerMovementSync.TargetForceRotation(Connection, -euler.x, true, euler.y, true);
        }
    }
    //TODO:
    /*

    /// <summary>
    /// The Rotation of the Player as vector2
    /// </summary>
    public virtual Vector2 RotationVector2
    {
        get => PlayerMovementSync.Rotations;
        set => PlayerMovementSync.TargetForceRotation(Connection, -value.x, true, value.y, true);
    }

    public virtual float RotationFloat
    {
        get => RotationVector2.y;
        set => PlayerMovementSync.TargetForceRotation(Connection, 0f, false, value, true);
    }
    */
    
    //TODO:
    /*
    /// <summary>
    /// The Rotation of the Player as PlayerRotation
    /// </summary>
    public virtual PlayerMovementSync.PlayerRotation PlayerRotation
    {
        get
        {
            var vec2 = RotationVector2;
            return new PlayerMovementSync.PlayerRotation(vec2.x, vec2.y);
        }
        set => PlayerMovementSync.TargetForceRotation(Connection, -(value.x ?? 0f), true, value.y ?? 0f, true);
    }
    */

    public Vector3 Scale
    {
        get => transform.localScale;
        set
        {
            transform.localScale = value;
            
            foreach (var ply in _player.Players)
                NetworkServer.SendSpawnMessage(NetworkIdentity, ply.Connection);
        }
    }

    /// <summary>
    /// The Room the Player is currently inside
    /// </summary>
    public IRoom Room
    {
        get => _room.GetNearestRoom(Position);
        set => Position = value.Position;
    }

    /// <summary>
    /// The Current Position and Rotation of the Player as RoomPoint
    /// </summary>
    public RoomPoint RoomPoint
    {
        get => new(Position, Rotation);
        set
        {
            Position = value.GetMapPosition();
            Rotation = value.GetMapRotation();
        }
    }

    public uint ZoneId => Room.Zone;
}