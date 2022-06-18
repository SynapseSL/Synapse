using Hints;
using InventorySystem;
using InventorySystem.Searching;
using Mirror;
using PlayerStatsSystem;
using RemoteAdmin;
using Synapse3.SynapseModule.Enums;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// The Vanilla Player class
    /// </summary>
    public ReferenceHub Hub { get; }

    public Transform CameraReference => Hub.PlayerCameraReference;
    
    public NetworkIdentity NetworkIdentity => Hub.networkIdentity;

    public NetworkConnection Connection => ClassManager.Connection;

    public LocalCurrentRoomEffects LocalCurrentRoomEffects => Hub.localCurrentRoomEffects;

    public HintDisplay HintDisplay => Hub.hints;

    public SearchCoordinator SearchCoordinator => Hub.searchCoordinator;

    public FootstepSync FootstepSync => Hub.footstepSync;

    public PlayerEffectsController PlayerEffectsController => Hub.playerEffectsController;

    public PlayerInteract PlayerInteract => Hub.playerInteract;

    public FallDamage FallDamage => Hub.falldamage;

    public AnimationController AnimationController => Hub.animationController;

    public SpectatorManager SpectatorManager => Hub.spectatorManager;

    public NicknameSync NicknameSync => Hub.nicknameSync;

    public PlayerMovementSync PlayerMovementSync => Hub.playerMovementSync;

    public QueryProcessor QueryProcessor => Hub.queryProcessor;

    public ServerRoles ServerRoles => Hub.serverRoles;

    public PlayerStats PlayerStats => Hub.playerStats;

    public Inventory VanillaInventory => Hub.inventory;

    public CharacterClassManager ClassManager => Hub.characterClassManager;
    
    public Assets._Scripts.Dissonance.DissonanceUserSetup DissonanceUserSetup { get; }

    public Radio Radio { get; }

    public GameConsoleTransmission GameConsoleTransmission { get; }

    public Escape Escape { get; }
    
    public CommandSender CommandSender
    {
        get
        {
            if (PlayerType == PlayerType.Server) return ServerConsole._scs;
            return QueryProcessor._sender;
        }
    }

    /// <summary>
    /// Returns the PlayerStatBase of the specific Type
    /// </summary>
    public TStat GetStatBase<TStat>() where TStat : StatBase => PlayerStats.GetModule<TStat>();
}