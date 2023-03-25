using Mirror;
using PlayerRoles.FirstPersonControl;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{

    internal bool refreshVerticalRotation = false;
    internal bool refreshHorizontalRotation = false;

    internal float verticalRotation = 0;
    internal float horizontalRotation = 0;

    /// <summary>
    /// If the player have a position in the game, not the case if the player have no player model
    /// </summary>
    public bool ExistsInSpace => FirstPersonMovement != null;

    /// <summary>
    /// The first person movement module if the player have one
    /// </summary>
    public FirstPersonMovementModule FirstPersonMovement
    {
        get
        {
            if (CurrentRole is IFpcRole fpcRole && fpcRole.FpcModule.ModuleReady)
                return fpcRole.FpcModule;
            return null;
        }
    }

    /// <summary>
    /// The current Position of the Player
    /// </summary>
    public virtual Vector3 Position
    {
        get => FirstPersonMovement?.Position ?? Vector3.zero;
        set => FirstPersonMovement.ServerOverridePosition(value, new Vector3(0f, 0f, 0f));
    }

    /// <summary>
    /// The Rotation of the Player as Quaternion
    /// Note: The rotation is only applied if it is different from the previous one!
    /// The precision of the rotation is 0.01 float
    /// </summary>
    public virtual Quaternion Rotation
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            return mouseLook == null
                ? new Quaternion(0, 0, 0, 0)
                : Quaternion.Euler(mouseLook.CurrentVertical,mouseLook.CurrentHorizontal, 0f);
        }
        set
        {
            refreshHorizontalRotation = true;
            refreshVerticalRotation = true;
            verticalRotation = value.eulerAngles.y;
            horizontalRotation = value.eulerAngles.x;
        }
    }

    /// <summary>
    /// The Rotation of the Player as vector2
    /// Note: The rotation is only applied if it is different from the previous one!
    /// The precision of the rotation is 0.01 float
    /// </summary>
    public virtual Vector2 RotationVector2
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            return mouseLook == null
                ? Vector2.zero
                : new Vector2(mouseLook.CurrentVertical,mouseLook.CurrentHorizontal);
        }
        set
        {
            refreshHorizontalRotation = true;
            refreshVerticalRotation = true;
            verticalRotation = value.y;
            horizontalRotation = value.x;
        }
    }


    /// <summary>
    /// The Rotation of the Player on the y axe (min 0, max 360)
    /// Note: The rotation is only applied if it is different from the previous one!
    /// The precision of the rotation is 0.01 float
    /// </summary>
    public virtual float RotationHorizontal
    { 
        get => FirstPersonMovement?.MouseLook.CurrentHorizontal ?? 0;
        set
        {
            refreshHorizontalRotation = true;
            horizontalRotation = value;
        }
    }
    /// <summary>
    /// The Rotation of the Player on the x axe (min -88, max 88)
    /// Note: The rotation is only applied if it is different from the previous one!
    /// The precision of the rotation is 0.01 float
    /// </summary>
    public virtual float RotationVertical
    {
        get => FirstPersonMovement?.MouseLook.CurrentVertical ?? 0;
        set
        {
            refreshVerticalRotation = true;
            verticalRotation = value;
        }
    }

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
            //Not possible currently
            //Rotation = value.GetMapRotation();
        }
    }

    public uint ZoneId => Room.Zone;
}