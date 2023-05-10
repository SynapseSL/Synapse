using System.Linq;
using System.Reflection;
using GameCore;
using InventorySystem.Disarming;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Spectating;
using PlayerStatsSystem;
using RelativePositioning;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Teams;
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

    public uint NetId => NetworkIdentity.netId;

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
        get => GetStatBase<AdminFlagsStat>().HasFlag(AdminFlags.Noclip);
        set => GetStatBase<AdminFlagsStat>().SetFlag(AdminFlags.Noclip, value);
    }

    /// <summary>
    /// If the Player is allowed to change NoClip Mode on his own
    /// </summary>
    public bool NoClipPermitted
    {
        get => FpcNoclip.IsPermitted(Hub);
        set
        {
            if(value)
                FpcNoclip.PermitPlayer(Hub);
            else
                FpcNoclip.UnpermitPlayer(Hub);
        }
    }

    /// <summary>
    /// When Enabled the player will be constantly a Spectator
    /// </summary>
    public bool OverWatch
    {
        get => CurrentRole is OverwatchRole;
        set => SetRoleFlags(value ? RoleTypeId.Overwatch : RoleTypeId.Spectator, RoleSpawnFlags.All,
            RoleChangeReason.RemoteAdmin);
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

    /// <summary>
    /// The Time the player died
    /// </summary>
    public float DeathTime => (CurrentRole as SpectatorRole)?.ActiveTime ?? 0;

    /// <summary>
    /// The current movement of the player
    /// </summary>
    public PlayerMovementState MovementState =>
        FirstPersonMovement?.CurrentMovementState ?? PlayerMovementState.Walking;

    /// <summary>
    /// The current health of the player
    /// </summary>
    public float Health
    {
        get => GetStatBase<HealthStat>().CurValue;
        set => GetStatBase<HealthStat>().CurValue = value;
    }

    internal float _maxHealth { get; set; } = -1;

    /// <summary>
    /// The maximum health a player can have
    /// </summary>
    public float MaxHealth
    {
        get => _maxHealth == -1 ? CurrentRole is IHealthbarRole healthRole ? healthRole.MaxHealth : 0 : _maxHealth;
        set => _maxHealth = value;
    }

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


    /// <summary>
    /// The current stamina of the player
    /// </summary>
    public float Stamina
    {
        get => GetStatBase<StaminaStat>().CurValue * 100;
        set => GetStatBase<StaminaStat>().CurValue = value / 100;
    }
    
    /// <summary>
    /// The curent stamina use by the player (set to -1 to use the default one)
    /// </summary>
    public float StaminaUseRate
    {
        get => (CurrentRole as FpcStandardRoleBase)?.FpcModule?.StateProcessor?._useRate ?? 0;
        set
        {
            if (CurrentRole is not FpcStandardRoleBase fpcRole) return;
            if (value < 0) value = ConfigFile.ServerConfig.GetFloat("stamina_balance_use", 0.05f);
            typeof(FpcStateProcessor)
                .GetField(nameof(FpcStateProcessor._useRate), BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(fpcRole.FpcModule.StateProcessor, value);
        }
    }

    public string UnitName => (CurrentRole as HumanRole)?.UnitName ?? "";

    public byte UnitNameId
    {
        get => (CurrentRole as HumanRole)?.UnitNameId ?? 0;
        set
        {
            if(CurrentRole is not HumanRole role) return;
            role.UnitNameId = value;
        }
    }

    /// <summary>
    /// The player who is spectated by the player
    /// </summary>
    public SynapsePlayer CurrentlySpectating
    {
        get
        {
            if (CurrentRole is not SpectatorRole role) return null;
            if (role.SyncedSpectatedNetId == 0) return null;
            return _player.GetPlayer(role.SyncedSpectatedNetId);
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
    public GameObject LookingAt =>
        !Physics.Raycast(CameraReference.transform.position, CameraReference.transform.forward,
            out RaycastHit raycastHit, 100f) ? null : raycastHit.transform.gameObject;

    /// <summary>
    /// True if the player has DoNotTrack enabled (https://scpslgame.com/Verified_server_rules.pdf [8.11])
    /// </summary>
    public bool DoNotTrack => ServerRoles.DoNotTrack;

    /// <summary>
    /// True if the player is disarmed
    /// </summary>
    public bool IsDisarmed => VanillaInventory.IsDisarmed();

    public bool IsAlive => Hub.IsAlive();

    public bool IsHuman => Hub.IsHuman();

    /// <summary>
    /// The time a player is alive
    /// </summary>
    public float AliveTime => CurrentRole is SpectatorRole or NoneRole ? 0f : CurrentRole.ActiveTime;

    /// <summary>
    /// I don't need to explain this, right?
    /// </summary>
    public int Ping => LiteNetLib4MirrorServer.Peers[Connection.connectionId].Ping;

    public ISynapseTeam CustomTeam => CustomRole == null ? null : _team.Teams.FirstOrDefault(x => x.Attribute.Id == TeamID);
    
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

    /// <summary>
    /// The sneak speed of the curent roleTypeID
    /// </summary>
    public virtual float SneakSpeed => FirstPersonMovement?.SneakSpeed ?? 0;

    /// <summary>
    /// The sneak warlk of the curent roleTypeID
    /// </summary>
    public virtual float WalkSpeed => FirstPersonMovement?.WalkSpeed ?? 0;

    /// <summary>
    /// The sneak run of the curent roleTypeID
    /// </summary>
    public virtual float RunSpeed => FirstPersonMovement?.SprintSpeed ?? 0;

    /// <summary>
    /// The sneak crouching of the curent roleTypeID
    /// </summary>
    public virtual float CrouchingSpeed => FirstPersonMovement?.CrouchSpeed ?? 0;
}