using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PluginAPI.Core;
using Synapse3.SynapseModule.Map.Rooms;
using System;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    void UpdateRotation()
    {
        foreach (var player in _player.Players)
            this.SendNetworkMessage(new FpcPositionMessage(player));
    }

    /// <summary>
    /// If the player ave a postion in the game, not the case if the player ave no player model
    /// </summary>
    public bool ExistsInSpace => FirstPersonMovement != null;

    /// <summary>
    /// The frist person movement module if the player ave one
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
    /// </summary>
    public virtual Quaternion Rotation
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            if (mouseLook == null)
                return new Quaternion(0, 0, 0, 0);
            return Quaternion.Euler(mouseLook.CurrentHorizontal, mouseLook.CurrentVertical, 0f);
        }
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;
            var euler = value.eulerAngles;
            firstperson.MouseLook.CurrentHorizontal = -euler.x;
            firstperson.MouseLook.CurrentVertical = euler.y;
            UpdateRotation();
        }
    }

    /// <summary>
    /// The Rotation of the Player as vector2
    /// </summary>
    public virtual Vector2 RotationVector2
    {
        get
        {
            var mouseLook = FirstPersonMovement?.MouseLook;
            if (mouseLook == null)
                return Vector2.zero;
            return new Vector2(mouseLook.CurrentHorizontal, mouseLook.CurrentVertical);
        } 
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;

            firstperson.MouseLook.CurrentHorizontal = value.x;
            firstperson.MouseLook.CurrentVertical = value.y;
            UpdateRotation();
        }
    }


    /// <summary>
    /// The Rotation of the Player on the X axe (min 0, max 360)
    /// </summary>
    public virtual float RotationHorizontal
    {
        get => FirstPersonMovement?.MouseLook.CurrentHorizontal ?? 0;
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;
            var delta = value - RotationHorizontal;
            FirstPersonMovement.ServerOverridePosition(Position, new Vector3(delta, 0f, 0f));
        }
    }

    /// <summary>
    /// The Rotation of the Player on the Y axe (min -88, max 88)
    /// </summary>
    public virtual float RotationVectical
    {
        get => FirstPersonMovement?.MouseLook.CurrentVertical ?? 0;
        set
        {
            var firstperson = FirstPersonMovement;
            if (firstperson == null) return;
            firstperson.MouseLook.CurrentVertical = value;
            UpdateRotation();
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
            Rotation = value.GetMapRotation();
        }
    }

    public uint ZoneId => Room.Zone;
}