using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public string NickName => NicknameSync.Network_myNickSync;
    
    public string DisplayName
    {
        get => NicknameSync.DisplayName;
        set => NicknameSync.DisplayName = value;
    }

    public string DisplayInfo
    {
        get => NicknameSync._customPlayerInfoString;
        set => NicknameSync.Network_customPlayerInfoString = value;
    }
    
    public int PlayerId
    {
        get => QueryProcessor.PlayerId;
        set => QueryProcessor.NetworkPlayerId = value;
    }
    
    public string UserId
    {
        get => ClassManager.UserId;
        set => ClassManager.UserId = value;
    }

    public string SecondUserID
    {
        get => ClassManager.UserId2;
        set => ClassManager.UserId2 = value;
    }
    
    public bool NoClip
    {
        get => ServerRoles.NoclipReady;
        set => ServerRoles.NoclipReady = value;
    }

    public bool OverWatch
    {
        get => ServerRoles.OverwatchEnabled;
        set => ServerRoles.OverwatchEnabled = value;
    }

    public bool Bypass
    {
        get => ServerRoles.BypassMode;
        set => ServerRoles.BypassMode = value;
    }

    public bool GodMode
    {
        get => ClassManager.GodMode;
        set => ClassManager.GodMode = value;
    }

    public bool Invisible { get; set; }

    public Vector3 Position
    {
        get => PlayerMovementSync.GetRealPosition();
        set => PlayerMovementSync.OverridePosition(value, PlayerRotation);
    }

    public Vector2 Rotation
    {
        get => PlayerMovementSync.RotationSync;
        set => PlayerMovementSync.NetworkRotationSync = value;
    }
    
    public PlayerMovementSync.PlayerRotation PlayerRotation
    {
        get
        {
            var vec2 = Rotation;
            return new PlayerMovementSync.PlayerRotation(vec2.x, vec2.y);
        }
        set => Rotation = new Vector2(value.x.Value, value.y.Value);
    }

    public Vector3 DeathPosition
    {
        get => ClassManager.DeathPosition;
        set => ClassManager.DeathPosition = value;
    }

    public long DeathTime
    {
        get => ClassManager.DeathTime;
        set => ClassManager.DeathTime = value;
    }

    public PlayerMovementState MovementState
    {
        get => AnimationController.MoveState;
        set => AnimationController.UserCode_CmdChangeSpeedState((byte)value);
    }
    
    public bool GlobalRemoteAdmin => ServerRoles.RemoteAdminMode == ServerRoles.AccessMode.GlobalAccess;
    
    public ulong GlobalPerms => ServerRoles._globalPerms;
}