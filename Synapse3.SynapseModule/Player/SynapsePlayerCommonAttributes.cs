using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// The Nickname of the Player he joines with
    /// </summary>
    public string NickName => NicknameSync.Network_myNickSync;
    
    /// <summary>
    /// The Current Displayed Name of the Player
    /// </summary>
    public string DisplayName
    {
        get => NicknameSync.DisplayName;
        set => NicknameSync.DisplayName = value;
    }

    /// <summary>
    /// Custom Info that will be displayed below the players name
    /// </summary>
    public string DisplayInfo
    {
        get => NicknameSync._customPlayerInfoString;
        set => NicknameSync.Network_customPlayerInfoString = value;
    }
    
    /// <summary>
    /// The PlayerID of the Player that the server assigned
    /// </summary>
    public int PlayerId
    {
        get => QueryProcessor.PlayerId;
        set => QueryProcessor.NetworkPlayerId = value;
    }
    
    /// <summary>
    /// The UserID of the player like 1234@steam 1234@discord 1234@patreon 1234@northwood
    /// </summary>
    public string UserId
    {
        get => ClassManager.UserId;
        set => ClassManager.UserId = value;
    }

    /// <summary>
    /// A potentially second id of the User.It is most often used when a custom ID is present like 1234@patreon
    /// </summary>
    public string SecondUserID
    {
        get => ClassManager.UserId2;
        set => ClassManager.UserId2 = value;
    }
    
    /// <summary>
    /// When Enabled the Player can fly through walls
    /// </summary>
    public bool NoClip
    {
        get => ServerRoles.NoclipReady;
        set => ServerRoles.NoclipReady = value;
    }

    /// <summary>
    /// When Enabled the player will be constantly a Spectator
    /// </summary>
    public bool OverWatch
    {
        get => ServerRoles.OverwatchEnabled;
        set => ServerRoles.OverwatchEnabled = value;
    }

    /// <summary>
    /// When Enabled the Player can Bypass most checks like KeyCard Permission on doors
    /// </summary>
    public bool Bypass
    {
        get => ServerRoles.BypassMode;
        set => ServerRoles.BypassMode = value;
    }

    /// <summary>
    /// When Enabled the Player cant take damage
    /// </summary>
    public bool GodMode
    {
        get => ClassManager.GodMode;
        set => ClassManager.GodMode = value;
    }

    /// <summary>
    /// When enabled only players with the synapse.invisible Permission can see the player
    /// </summary>
    public bool Invisible { get; set; }

    /// <summary>
    /// The current Position of the Player
    /// </summary>
    public Vector3 Position
    {
        get => PlayerMovementSync.GetRealPosition();
        set => PlayerMovementSync.OverridePosition(value, PlayerRotation);
    }

    /// <summary>
    /// The Rotation of the Player as Quaternion
    /// </summary>
    public Quaternion Rotation => transform.rotation;

    /// <summary>
    /// The Rotation of the Player as vector2
    /// </summary>
    public Vector2 RotationVector2
    {
        get => PlayerMovementSync.RotationSync;
        set => PlayerMovementSync.NetworkRotationSync = value;
    }
    
    /// <summary>
    /// The Rotation of the Player as PlayerRotation
    /// </summary>
    public PlayerMovementSync.PlayerRotation PlayerRotation
    {
        get
        {
            var vec2 = RotationVector2;
            return new PlayerMovementSync.PlayerRotation(vec2.x, vec2.y);
        }
        set => RotationVector2 = new Vector2(value.x.Value, value.y.Value);
    }

    /// <summary>
    /// The last position the player died. Used to revive him as SCP-049-2
    /// </summary>
    public Vector3 DeathPosition
    {
        get => ClassManager.DeathPosition;
        set => ClassManager.DeathPosition = value;
    }

    /// <summary>
    /// The Time the player died
    /// </summary>
    public long DeathTime
    {
        get => ClassManager.DeathTime;
        set => ClassManager.DeathTime = value;
    }

    /// <summary>
    /// The current movement of the player
    /// </summary>
    public PlayerMovementState MovementState
    {
        get => AnimationController.MoveState;
        set => AnimationController.UserCode_CmdChangeSpeedState((byte)value);
    }
    
    /// <summary>
    /// If the Player has Globally Permissions for RemoteAdmin
    /// </summary>
    public bool GlobalRemoteAdmin => ServerRoles.RemoteAdminMode == ServerRoles.AccessMode.GlobalAccess;
    
    /// <summary>
    /// The Global Permissions of the Player
    /// </summary>
    public ulong GlobalPerms => ServerRoles._globalPerms;
    
    /// <summary>
    /// Freezes the Player in his current location
    /// </summary>
    public bool StopInput { get => Hub.fpc.NetworkforceStopInputs; set => Hub.fpc.NetworkforceStopInputs = value; }
}