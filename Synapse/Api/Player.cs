using System;
using System.Linq;
using Hints;
using Mirror;
using RemoteAdmin;
using Searching;
using UnityEngine;

namespace Synapse.Api
{
    public class Player : MonoBehaviour
    {
        internal Player()
        {
            Hub = GetComponent<ReferenceHub>();
        }

        #region Special Stuff
        #endregion

        #region Synapse Api Objects
        #endregion

        #region Default Stuff
        public string DisplayName 
        { 
            get => NicknameSync.DisplayName; 
            set => NicknameSync.DisplayName = value; 
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

        public Vector3 Position
        {
            get => PlayerMovementSync.GetRealPosition();
            set => PlayerMovementSync.OverridePosition(value, 0f);
        }

        public Vector2 Rotation
        {
            get => PlayerMovementSync.RotationSync;
            set => PlayerMovementSync.RotationSync = value;
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

        public Vector3 Scale
        {
            get => transform.localScale;
            set
            {
                try
                {
                    transform.localScale = value;

                    foreach(var player in SynapseController.Server.Players)
                        typeof(NetworkServer).GetMethod("SendSpawnMessage")?.Invoke(null, new object[] { GetComponent<NetworkIdentity>(), player.Connection});
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Player: SetScale failed!!\n{e}");
                }
            }
        }

        public float Health
        {
            get => PlayerStats.Health;
            set => PlayerStats.Health = value;
        }

        public int MaxHealth
        {
            get => PlayerStats.maxHP;
            set => PlayerStats.maxHP = value;
        }

        public float ArtificialHealth
        {
            get => PlayerStats.unsyncedArtificialHealth;
            set => PlayerStats.unsyncedArtificialHealth = value;
        }

        public int MaxArtificialHealth
        {
            get => PlayerStats.maxArtificialHealth;
            set => PlayerStats.maxArtificialHealth = value;
        }

        public RoleType Role
        {
            get => ClassManager.CurClass;
            set => ClassManager.SetPlayersClass(value, gameObject);
        }

        public Room Room
        {
            get => SynapseController.Server.Map.Rooms.OrderBy(x => Vector3.Distance(x.Position, Position)).FirstOrDefault();
            set => Position = value.Position;
        }

        public MapPoint MapPoint
        {
            get => new MapPoint(Room, Position);
            set => Position = value.Position;
        }

        public Inventory.SyncListItemInfo Items
        {
            get => Inventory.items;
            set => Inventory.items = value;
        }

        public Player Cuffer
        {
            //TODO: Player.GetCuffer
            //get => GetPlayer(Handcuffs.CufferId);
            set
            {

                var handcuff = value.Handcuffs;

                if (handcuff == null) return;

                if (value == null)
                {
                    Handcuffs.NetworkCufferId = -1;
                    return;
                }

                Handcuffs.NetworkCufferId = value.PlayerId;
            }
        }

        public GameObject LookingAt
        {
            get
            {
                if (!Physics.Raycast(CameraReference.transform.position, CameraReference.transform.forward, out RaycastHit raycastthit, 100f))
                    return null;

                return raycastthit.transform.gameObject;
            }
        }

        public uint Ammo5 
        { 
            get => AmmoBox.amount[0]; 
            set => AmmoBox.amount[0] = value; 
        }

        public uint Ammo7 
        { 
            get => AmmoBox.amount[1]; 
            set => AmmoBox.amount[1] = value; 
        }

        public uint Ammo9 
        { 
            get => AmmoBox.amount[2]; 
            set => AmmoBox.amount[2] = value; 
        }



        public string NickName => NicknameSync.Network_myNickSync;

        public Team Team => ClassManager.CurRole.team;

        public Fraction Fraction => ClassManager.Fraction;

        public Inventory.SyncItemInfo ItemInHand => Inventory.GetItemInHand();

        public NetworkConnection Connection => QueryProcessor.connectionToClient;

        public string IpAddress => QueryProcessor._ipAddress;
        #endregion

        #region ReferenceHub
        public Transform CameraReference => Hub.PlayerCameraReference;

        public AmmoBox AmmoBox => Hub.ammoBox;

        public HintDisplay HintDisplay => Hub.hints;

        public SearchCoordinator SearchCoordinator => Hub.searchCoordinator;

        public FootstepSync FootstepSync => Hub.footstepSync;

        public PlayerEffectsController PlayerEffectsController => Hub.playerEffectsController;

        public PlayerInteract PlayerInteract => Hub.playerInteract;

        public Handcuffs Handcuffs => Hub.handcuffs;

        public FallDamage FallDamage => Hub.falldamage;

        public AnimationController AnimationController => Hub.animationController;

        public SpectatorManager SpectatorManager => Hub.spectatorManager;

        public NicknameSync NicknameSync => Hub.nicknameSync;

        public PlayerMovementSync PlayerMovementSync => Hub.playerMovementSync;

        public QueryProcessor QueryProcessor => Hub.queryProcessor;

        public ServerRoles ServerRoles => Hub.serverRoles;

        public PlayerStats PlayerStats => Hub.playerStats;

        public Inventory Inventory => Hub.inventory;

        public CharacterClassManager ClassManager => Hub.characterClassManager;
        
        public ReferenceHub Hub { get; internal set; }
        #endregion

        public override string ToString() => NickName;
    }
}
