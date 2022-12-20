using System.Linq;
using InventorySystem.Disarming;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Spectating;
using PlayerStatsSystem;
using RelativePositioning;
using Synapse3.SynapseModule.Enums;
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
    public CustomInfoList CustomInfo { get; set; }
    
    /// <summary>
    /// The PlayerID of the Player that the server assigned
    /// </summary>
    public int PlayerId
    {
        get => Hub.PlayerId;
        set => Hub._playerId = new RecyclablePlayerId(value);
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
        get => ServerRoles._noclipReady;
        set => ServerRoles._noclipReady = value;
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
    /// The Current Invisible Mode of the Player
    /// </summary>
    public InvisibleMode Invisible { get; set; } = InvisibleMode.None;

    /// <summary>
    /// The last position the player died. Used to revive him as SCP-049-2
    /// </summary>
    public Vector3 DeathPosition
    {
        get => (CurrentRole as SpectatorRole)?.DeathPosition.Position ?? Vector3.zero;
        set
        {
            if (CurrentRole is SpectatorRole role) role.DeathPosition = new RelativePosition(value);
        }
    }

    //TODO:
    /*
    /// <summary>
    /// The Time the player died
    /// </summary>
    public long DeathTime
    {
        get => ClassManager
        set => ClassManager.DeathTime = value;
    }
    */

    /// <summary>
    /// The current movement of the player
    /// </summary>
    public PlayerMovementState MovementState
    {
        get => AnimationController.MoveState;
        set => AnimationController.UserCode_CmdChangeSpeedState((byte)value);
    }

    //TODO:
    /*
    /// <summary>
    /// Freezes the Player in his current location
    /// </summary>
    public bool StopInput { get => Hub.fpc.NetworkforceStopInputs; set => Hub.fpc.NetworkforceStopInputs = value; }
    */

    /// <summary>
    /// The current health of the player
    /// </summary>
    public float Health
    {
        get => GetStatBase<HealthStat>().CurValue;
        set => GetStatBase<HealthStat>().CurValue = value;
    }

    /// <summary>
    /// The maximum health a player can have
    /// </summary>
    public float MaxHealth { get; set; } = 100f;

    /// <summary>
    /// The current artificial health of the player
    /// </summary>
    public float ArtificialHealth
    {
        get => GetStatBase<AhpStat>().CurValue;
        set => GetStatBase<AhpStat>().ServerAddProcess(value, MaxArtificialHealth, DecayArtificialHealth, 0f, 0f, false);
    }

    private float _maxAhp = AhpStat.DefaultMax;
    /// <summary>
    /// The maximum artificial health a player can have
    /// </summary>
    public float MaxArtificialHealth
    {
        get => _maxAhp;
        set
        {
            _maxAhp = value;
            foreach (var process in GetStatBase<AhpStat>()._activeProcesses)
            {
                process.Limit = value;
            }
        }
    }

    private float _decayArtificialHealth = AhpStat.DefaultDecay;
    public float DecayArtificialHealth
    {
        get => _decayArtificialHealth;
        set
        {
            _decayArtificialHealth = value;
            foreach (var process in GetStatBase<AhpStat>()._activeProcesses)
            {
                process.DecayRate = value;
            }
        }
    }

    //TODO:
    /*
    /// <summary>
    /// The current stamina of the player
    /// </summary>
    public float Stamina
    {
        get => Hub.fpc.staminaController.RemainingStamina * 100;
        set => Hub.fpc.staminaController.RemainingStamina = value / 100;
    }

    /// <summary>
    /// The stamina usage of the player
    /// </summary>
    public float StaminaUsage
    {
        get => Hub.fpc.staminaController.StaminaUse * 100;
        set => Hub.fpc.staminaController.StaminaUse = value / 100;
    }

   
    public byte UnitId
    {
        get => ClassManager. CurSpawnableTeamType;
        set
        {
            ClassManager.CurSpawnableTeamType = value;
            SendNetworkMessage(_mirror.GetCustomVarMessage(ClassManager, writer =>
            {
                writer.WriteUInt64(16ul);
                writer.WriteByte(value);
            }));
        }
    }

    public string Unit
    {
        get => ClassManager.CurUnitName;
        set
        {
            ClassManager.CurUnitName = value;
            SendNetworkMessage(_mirror.GetCustomVarMessage(ClassManager, writer =>
            {
                writer.WriteUInt64(32ul);
                writer.WriteString(value);
            }));
        }
    }


    /// <summary>
    /// The player who is spectated by the player
    /// </summary>
    public SynapsePlayer CurrentlySpectating
    {
        get => SpectatorManager.CurrentSpectatedPlayer != null ? SpectatorManager.CurrentSpectatedPlayer.GetSynapsePlayer() : null;
        set => SpectatorManager.CurrentSpectatedPlayer = value;
    }
    */

    public float SneakSpeed
    {
        get => FirstPersonMovement?.SneakSpeed ?? 0;
        set
        {
            var firstperosn = FirstPersonMovement;
            if (firstperosn != null)
                firstperosn.SneakSpeed = value;
        }
    }

    public float WalkSpeed
    {
        get => FirstPersonMovement?.WalkSpeed ?? 0;
        set
        {
            var firstperosn = FirstPersonMovement;
            if (firstperosn != null)
                firstperosn.WalkSpeed = value;
        }
    }

    public float RunSpeed
    {
        get => FirstPersonMovement?.SprintSpeed ?? 0;
        set
        {
            var firstperosn = FirstPersonMovement;
            if (firstperosn != null)
                firstperosn.SprintSpeed = value;
        }
    }
    public float CrouchingSpeed
    {
        get => FirstPersonMovement?.CrouchSpeed ?? 0;
        set
        {
            var firstperosn = FirstPersonMovement;
            if (firstperosn != null)
                firstperosn.CrouchSpeed = value;
        }
    }

    /// <summary>
    /// The player who disarmed the player
    /// </summary>
    public SynapsePlayer Disarmer
    {
        get
        {
            if (DisarmedPlayers.Entries.All(x => x.DisarmedPlayer != NetworkIdentity.netId)) return null;

            var id = DisarmedPlayers.Entries.FirstOrDefault(x => x.DisarmedPlayer == NetworkIdentity.netId).Disarmer;
            if (id == 0) return ReferenceHub.LocalHub.GetSynapsePlayer();

            _player.GetPlayer(id);
            return null;
        }
        set => VanillaInventory.SetDisarmedStatus(value.VanillaInventory);
    }

    /// <summary>
    /// The gameobject the player is looking at
    /// </summary>
    public GameObject LookingAt
    {
        get
        {
            if (!Physics.Raycast(CameraReference.transform.position, CameraReference.transform.forward,
                    out RaycastHit raycastHit, 100f)) return null;
            return raycastHit.transform.gameObject;
        }
    }

    /// <summary>
    /// True if the player has DoNotTrack enabled (https://scpslgame.com/Verified_server_rules.pdf [8.11])
    /// </summary>
    public bool DoNotTrack => ServerRoles.DoNotTrack;

    /// <summary>
    /// True if the player is disarmed
    /// </summary>
    public bool IsDisarmed => VanillaInventory.IsDisarmed();

    //TODO:
    /*
    /// <summary>
    /// The time a player is alive
    /// </summary>
    public float AliveTime => ClassManager.alive
    */

    /// <summary>
    /// I don't need to explain this, right?
    /// </summary>
    public int Ping => LiteNetLib4MirrorServer.Peers[Connection.connectionId].Ping;

    /// <summary>
    /// The current Team of the player
    /// </summary>
    public Team Team => CurrentRole.Team;

    /// <summary>
    /// The current team id of the player
    /// </summary>
    public uint TeamID => CustomRole?.Attribute?.TeamId ?? (uint)Team;

    /// <summary>
    /// The current faction of the player
    /// </summary>
    public Faction Faction => CurrentRole.Team.GetFaction();

    /// <summary>
    /// The ip address of the player
    /// </summary>
    public string IpAddress => QueryProcessor._ipAddress;
}