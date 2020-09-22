using System;
using System.Linq;
using Hints;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using RemoteAdmin;
using Searching;
using Synapse.Api.Enums;
using Synapse.Api.Roles;
using Synapse.Database;
using Synapse.Patches.EventsPatches.PlayerPatches;
using UnityEngine;

namespace Synapse.Api
{
    public class Player : MonoBehaviour
    {
        internal Player()
        {
            Hub = GetComponent<ReferenceHub>();
            Scp106Controller = new Scp106Controller(this);
            Scp079Controller = new Scp079Controller(this);
            Jail = new Jail(this);
        }

        #region Methods
        public void Kick(string message) => ServerConsole.Disconnect(gameObject, message);

        public void Ban(int duration, string reason, string issuer = "Plugin") => SynapseController.Server.GetObjectOf<BanPlayer>().BanUser(gameObject, duration, reason, issuer);

        public void ChangeRoleAtPosition(RoleType role)
        {
            RoleChangeClassIdPatch.ForceLite = true;
            Hub.characterClassManager.SetClassIDAdv(role, true);
        }

        public void Kill(DamageTypes.DamageType damageType = default) => PlayerStats.HurtPlayer(new PlayerStats.HitInfo(-1f, "WORLD", damageType, 0), gameObject);

        public void GiveTextHint(string message, float duration = 5f)
        {
            Hub.hints.Show(new TextHint(message, new HintParameter[]
                {
                    new StringHintParameter("")
                }, HintEffectPresets.FadeInAndOut(duration), duration));
        }

        public void ClearBroadcasts() => GetComponent<Broadcast>().TargetClearElements(Connection);

        public void Broadcast(ushort time, string message) => GetComponent<Broadcast>().TargetAddElement(Connection, message, time, new Broadcast.BroadcastFlags());

        public void InstantBroadcast(ushort time, string message)
        {
            ClearBroadcasts();
            GetComponent<Broadcast>().TargetAddElement(Connection, message, time, new Broadcast.BroadcastFlags());
        }

        public void SendConsoleMessage(string message, string color = "red") => ClassManager.TargetConsolePrint(Connection, message, color);

        public void SendRAConsoleMessage(string message, bool success = true, RaCategory type = RaCategory.None) => SynapseExtensions.RaMessage(CommandSender,message, success, type);

        public void GiveItem(ItemType itemType, float duration = float.NegativeInfinity, int sight = 0, int barrel = 0, int other = 0) => Inventory.AddNewItem(itemType, duration, sight, barrel, other);

        public void DropAllItems() => Inventory.ServerDropAll();

        public void DropItem(Inventory.SyncItemInfo item)
        {
            Inventory.SetPickup(item.id, item.durability, Position, Inventory.camera.transform.rotation, item.modSight, item.modBarrel, item.modOther);
            Items.Remove(item);
        }

        public void ClearInventory() => Inventory.Clear();

        public void GiveEffect(Effect effect, byte intensity = 1, float duration = -1f) => PlayerEffectsController.ChangeByString(effect.ToString().ToLower(), intensity, duration);

        public void RaLogin()
        {
            ServerRoles.RemoteAdmin = true;
            ServerRoles.RemoteAdminMode = ServerRoles.AccessMode.PasswordOverride;
            ServerRoles.TargetOpenRemoteAdmin(Connection, false);
        }

        public void RaLogout()
        {
            Hub.serverRoles.RemoteAdmin = false;
            Hub.serverRoles.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
            Hub.serverRoles.TargetCloseRemoteAdmin(Connection);
        }

        public void Hurt(int amount, DamageTypes.DamageType damagetype = default, Player attacker = null) =>
            PlayerStats.HurtPlayer(new PlayerStats.HitInfo(amount, attacker == null ? "WORLD" : attacker.NickName, damagetype, attacker == null ? PlayerId : attacker.PlayerId), attacker == null ? gameObject : attacker.gameObject);

        public void SendToServer(ushort port)
        {
            var component = SynapseController.Server.Host.PlayerStats;
            var writer = NetworkWriterPool.GetWriter();
            writer.WriteSingle(1f);
            writer.WriteUInt16(port);
            var msg = new RpcMessage
            {
                netId = component.netId,
                componentIndex = component.ComponentIndex,
                functionHash = typeof(PlayerStats).FullName.GetStableHashCode() * 503 + "RpcRoundrestartRedirect".GetStableHashCode(),
                payload = writer.ToArraySegment()
            };
            Connection.Send(msg);
            NetworkWriterPool.Recycle(writer);
        }

        public void DimScreen()
        {
            var component = RoundSummary.singleton;
            var writer = NetworkWriterPool.GetWriter();
            var msg = new RpcMessage
            {
                netId = component.netId,
                componentIndex = component.ComponentIndex,
                functionHash = typeof(RoundSummary).FullName.GetStableHashCode() * 503 + "RpcDimScreen".GetStableHashCode(),
                payload = writer.ToArraySegment()
            };
            Connection.Send(msg);
            NetworkWriterPool.Recycle(writer);
        }

        public void ShakeScreen(bool achieve = false)
        {
            var component = AlphaWarheadController.Host;
            var writer = NetworkWriterPool.GetWriter();
            writer.WriteBoolean(achieve);
            var msg = new RpcMessage
            {
                netId = component.netId,
                componentIndex = component.ComponentIndex,
                functionHash = typeof(AlphaWarheadController).FullName.GetStableHashCode() * 503 + "RpcShake".GetStableHashCode(),
                payload = writer.ToArraySegment()
            };
            Connection.Send(msg);
            NetworkWriterPool.Recycle(writer);
        }


        //TODO: Permission Check Method
        public bool HasPermission(string permission)
        {
            return true;
        }
        #endregion

        #region Synapse Api Objects

        public readonly Jail Jail;

        public readonly Scp106Controller Scp106Controller;

        public readonly Scp079Controller Scp079Controller;

        private IRole role;

        public IRole CustomRole
        {
            get => role;
            set
            {
                role = value;
                if (value == null)
                    return;

                role.Player = this;
                role.Spawn();
            }
        }
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

        public RoleType RoleType
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
            get => SynapseController.Server.GetPlayer(Handcuffs.CufferId);
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

        public UserGroup Rank 
        { 
            get => ServerRoles.Group; 
            set => ServerRoles.SetGroup(value, value != null && value.Permissions > 0UL, true); 
        }

        public string RankColor
        {
            get => Rank.BadgeColor;
            set => ServerRoles.SetColor(value);
        }

        public string RankName
        {
            get => Rank.BadgeText;
            set => ServerRoles.SetText(value);
        }

        public bool HideRank
        {
            get => string.IsNullOrEmpty(ServerRoles.HiddenBadge);
            set
            {
                if (value)
                    ClassManager.CmdRequestHideTag();
                else
                    ClassManager.CallCmdRequestShowTag(false);
            }
        }

        public ulong Permission 
        { 
            get => ServerRoles.Permissions; 
            set => ServerRoles.Permissions = value; 
        }

        public bool IsMuted 
        { 
            get => ClassManager.NetworkMuted; 
            set => ClassManager.NetworkMuted = value; 
        }

        public bool IsIntercomMuted
        { 
            get => ClassManager.NetworkIntercomMuted; 
            set => ClassManager.NetworkIntercomMuted = value; 
        }

        public string UnitName 
        { 
            get => ClassManager.NetworkCurUnitName; 
            set => ClassManager.NetworkCurUnitName = value; 
        }




        public CommandSender CommandSender
        {
            get
            {
                if (this == SynapseController.Server.Host) return ServerConsole._scs;
                return QueryProcessor._sender;
            }
        }

        public bool DoNotTrack => ServerRoles.DoNotTrack;

        public bool IsDead => Team == Team.RIP;

        public bool IsZooming => Hub.weaponManager.ZoomInProgress();

        public bool IsReloading => Hub.weaponManager.IsReloading();

        public bool IsCuffed => Cuffer != null;

        public float AliveTime => ClassManager.AliveTime;

        public string AuthToken => ClassManager.AuthToken;

        public int Ping => LiteNetLib4MirrorServer.Peers[Connection.connectionId].Ping;

        public string GroupName => ServerStatic.PermissionsHandler._members[UserId];

        public string NickName => NicknameSync.Network_myNickSync;

        public Team Team => ClassManager.CurRole.team;

        public Fraction Fraction => ClassManager.Fraction;

        public Inventory.SyncItemInfo ItemInHand => Inventory.GetItemInHand();

        public NetworkConnection Connection => ClassManager.Connection;

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

        public readonly ReferenceHub Hub;
        #endregion

        #region Persistence

        public string GetData(string key)
        {
            DatabaseManager.CheckEnabledOrThrow();
            var dbo = DatabaseManager.PlayerRepository.FindByGameId(UserId);
            return dbo.Data.ContainsKey(key) ? dbo.Data[key] : null;
        }

        public void SetData(string key, string value)
        {
            DatabaseManager.CheckEnabledOrThrow();
            var dbo = DatabaseManager.PlayerRepository.FindByGameId(UserId);
            dbo.Data[key] = value;
            if (value == null) dbo.Data.Remove(key);
            DatabaseManager.PlayerRepository.Save(dbo);
        }
        
        #endregion

        public override string ToString() => NickName;
    }
}
