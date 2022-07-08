using System;
using System.Linq;
using InventorySystem.Disarming;
using InventorySystem.Items;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerStatsSystem;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Rooms;
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
    /// Freezes the Player in his current location
    /// </summary>
    public bool StopInput { get => Hub.fpc.NetworkforceStopInputs; set => Hub.fpc.NetworkforceStopInputs = value; }
    
    /// <summary>
    /// The Current RoleType of the Player. Use RoleID instead if you want to set the Role of the Player and remove potentially active custom roles
    /// </summary>
    public virtual RoleType RoleType
    {
        get => ClassManager.CurClass;
        set => ClassManager.SetPlayersClass(value, gameObject, CharacterClassManager.SpawnReason.None);
    }

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
        set => GetStatBase<AhpStat>().ServerAddProcess(value, value, 1.2f, 0f, 0f, false);
    }

    private int maxahp = 75;
    /// <summary>
    /// The maximum artificial health a player can have
    /// </summary>
    public int MaxArtificialHealth
    {
        get => maxahp;
        set
        {
            maxahp = value;
            foreach (var process in GetStatBase<AhpStat>()._activeProcesses)
            {
                process.Limit = value;
            }
        }
    }

    /// <summary>
    /// The current stamina of the player
    /// </summary>
    public float Stamina
    {
        get => Hub.fpc.staminaController.RemainingStamina * 100;
        set => Hub.fpc.staminaController.RemainingStamina = (value / 100);
    }

    /// <summary>
    /// The stamina usage of the player
    /// </summary>
    public float StaminaUsage
    {
        get => Hub.fpc.staminaController.StaminaUse * 100;
        set => Hub.fpc.staminaController.StaminaUse = (value / 100);
    }

    /// <summary>
    /// The player who is spectated by the player
    /// </summary>
    public SynapsePlayer CurrentlySpectating
    {
        get => SpectatorManager.CurrentSpectatedPlayer != null ? SpectatorManager.CurrentSpectatedPlayer.GetPlayer() : null;
        set => SpectatorManager.CurrentSpectatedPlayer = value;
    }

    /// <summary>
    /// The player who cuffed the player
    /// </summary>
    public SynapsePlayer Cuffer
    {
        get
        {
            if (DisarmedPlayers.Entries.All(x => x.DisarmedPlayer != NetworkIdentity.netId)) return null;

            var id = DisarmedPlayers.Entries.FirstOrDefault(x => x.DisarmedPlayer == NetworkIdentity.netId).Disarmer;
            if (id == 0) return ReferenceHub.LocalHub.GetPlayer();

            Synapse.Get<PlayerService>().GetPlayer(id);
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
    /// True if the player is cuffed
    /// </summary>
    public bool IsCuffed => DisarmedPlayers.IsDisarmed(VanillaInventory);

    /// <summary>
    /// The time a player is alive
    /// </summary>
    public float AliveTime => ClassManager.AliveTime;

    /// <summary>
    /// I don't need to explain this, right?
    /// </summary>
    public int Ping => LiteNetLib4MirrorServer.Peers[Connection.connectionId].Ping;

    /// <summary>
    /// The current Team of the player
    /// </summary>
    public Team Team => ClassManager.CurRole.team;

    /// <summary>
    /// The current team id of the player
    /// </summary>
    public int TeamID => CustomRole?.GetTeamID() ?? (int)Team;

    /// <summary>
    /// The current faction of the player
    /// </summary>
    public Faction Faction => ClassManager.Faction;

    /// <summary>
    /// The ip address of the player
    /// </summary>
    public string IpAddress => QueryProcessor._ipAddress;
}