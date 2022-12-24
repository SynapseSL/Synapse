using CustomPlayerEffects;
using Hints;
using InventorySystem;
using InventorySystem.Searching;
using Mirror;
using PlayerRoles;
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

    public HintDisplay HintDisplay => Hub.hints;

    public SearchCoordinator SearchCoordinator => Hub.searchCoordinator;

    public PlayerEffectsController PlayerEffectsController => Hub.playerEffectsController;

    public PlayerInteract PlayerInteract => Hub.playerInteract;

    public NicknameSync NicknameSync => Hub.nicknameSync;

    public QueryProcessor QueryProcessor => Hub.queryProcessor;

    public ServerRoles ServerRoles => Hub.serverRoles;

    public PlayerStats PlayerStats => Hub.playerStats;

    public Inventory VanillaInventory => Hub.inventory;

    public CharacterClassManager ClassManager => Hub.characterClassManager;

    public PlayerRoleManager RoleManager => Hub.roleManager;

    public GameConsoleTransmission GameConsoleTransmission { get; }
    
    public global::Broadcast BroadcastController { get; }

    public CommandSender CommandSender
    {
        get
        {
            if (PlayerType == PlayerType.Server) return ServerConsole.Scs;
            return QueryProcessor._sender;
        }
    }

    /// <summary>
    /// Returns the PlayerStatBase of the specific Type
    /// </summary>
    public TStat GetStatBase<TStat>() where TStat : StatBase => PlayerStats.GetModule<TStat>();

    public TEffect GetEffect<TEffect>() where TEffect : StatusEffectBase => PlayerEffectsController.GetEffect<TEffect>();
}