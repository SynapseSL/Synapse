using System;
using System.Linq;
using System.Reflection;
using Hints;
using InventorySystem;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Searching;
using MapGeneration;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using RemoteAdmin;
using Synapse.Api.Enum;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using Synapse.Api.Roles;
using Synapse.Database;
using Synapse.Patches.EventsPatches.PlayerPatches;
using Synapse.Permission;
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
            Scp096Controller = new Scp096Controller(this);
            Scp173Controller = new Scp173Controller();
            Jail = new Jail(this);
            ActiveBroadcasts = new BroadcastList(this);
            Inventory = new PlayerInventory(this);
            GameConsoleTransmission = GetComponent<GameConsoleTransmission>();
            DissonanceUserSetup = GetComponent<Assets._Scripts.Dissonance.DissonanceUserSetup>();
            Radio = GetComponent<Radio>();
            Escape = GetComponent<Escape>();
            AmmoBox = new PlayerAmmoBox(this);
        }

        #region Methods
        
        [Obsolete("Use GetPreference()",true)]
        public int GetSightPreference(ItemType item) => GetPreference(item, 0);

        [Obsolete("Use GetPreference()", true)]
        public int GetBarrelPreference(ItemType item) => GetPreference(item, 1);

        [Obsolete("Use GetPreference()", true)]
        public int GetOtherPreference(ItemType item) => GetPreference(item, 2);

        [Obsolete("Use GetPreference(ItemType) without type", true)]
        private int GetPreference(ItemType item, int type) => (int)GetPreference(item);

        public uint GetPreference(ItemType item)
        {
            if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(Hub, out var dict) && dict.TryGetValue(item, out var result))
                return result;

            return 0;
        }

        public void Kick(string message) => ServerConsole.Disconnect(gameObject, message);

        public void Ban(int duration, string reason, string issuer = "Plugin") => SynapseController.Server.GetObjectOf<BanPlayer>().BanUser(gameObject, duration, reason, issuer);

        public void ChangeRoleAtPosition(RoleType role)
        {
            RoleChangeClassIdPatch.ForceLite = true;
            Hub.characterClassManager.SetClassIDAdv(role, true,CharacterClassManager.SpawnReason.None);
            RoleChangeClassIdPatch.ForceLite = false;
        }

        public void Kill(DamageTypes.DamageType damageType = default) => PlayerStats.HurtPlayer(new PlayerStats.HitInfo(-1f, "WORLD", damageType, 0, true), gameObject);

        public void GiveTextHint(string message, float duration = 5f)
        {
            Hub.hints.Show(new TextHint(message, new HintParameter[]
                {
                    new StringHintParameter("")
                }, HintEffectPresets.FadeInAndOut(duration), duration));
        }

        internal void ClearBroadcasts() => GetComponent<global::Broadcast>().TargetClearElements(Connection);

        internal void Broadcast(ushort time, string message) => GetComponent<global::Broadcast>().TargetAddElement(Connection, message, time, new global::Broadcast.BroadcastFlags());

        internal void InstantBroadcast(ushort time, string message)
        {
            ClearBroadcasts();
            GetComponent<global::Broadcast>().TargetAddElement(Connection, message, time, new global::Broadcast.BroadcastFlags());
        }

        public void SendConsoleMessage(string message, string color = "red") => ClassManager.TargetConsolePrint(Connection, message, color);

        public void SendRAConsoleMessage(string message, bool success = true, RaCategory type = RaCategory.None) => SynapseExtensions.RaMessage(CommandSender, message, success, type);

        public void GiveEffect(Effect effect, byte intensity = 1, float duration = -1f) => PlayerEffectsController.ChangeByString(effect.ToString().ToLower(), intensity, duration);

        public void RaLogin()
        {
            ServerRoles.RemoteAdmin = true;
            ServerRoles.Permissions = SynapseGroup.GetVanillaPermissionValue() | ServerRoles._globalPerms;
            ServerRoles.RemoteAdminMode = GlobalRemoteAdmin ? ServerRoles.AccessMode.GlobalAccess : ServerRoles.AccessMode.PasswordOverride;
            if (!ServerRoles.AdminChatPerms)
                ServerRoles.AdminChatPerms = SynapseGroup.HasVanillaPermission(PlayerPermissions.AdminChat);
            ServerRoles.TargetOpenRemoteAdmin(false);
        }

        public void RaLogout()
        {
            Hub.serverRoles.RemoteAdmin = false;
            Hub.serverRoles.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
            Hub.serverRoles.TargetCloseRemoteAdmin();
        }

        public void Heal(float hp) => PlayerStats.HealHPAmount(hp);

        public void Hurt(int amount, DamageTypes.DamageType damagetype = default, Player attacker = null)
        {
            if (attacker == null) attacker = this;
            attacker.PlayerStats.HurtPlayer(new PlayerStats.HitInfo(amount, attacker.NickName, damagetype, attacker.PlayerId, false), gameObject);
        }

        public void OpenReportWindow(string text) => GameConsoleTransmission.SendToClient(Connection, "[REPORTING] " + text, "white");

        public void RemoveDisplayInfo(PlayerInfoArea playerInfo) => NicknameSync.Network_playerInfoToShow &= ~playerInfo;

        public void AddDisplayInfo(PlayerInfoArea playerInfo) => NicknameSync.Network_playerInfoToShow |= playerInfo;

        public void ExecuteCommand(string command, bool RA = true)
        {
            if (RA) RemoteAdmin.CommandProcessor.ProcessQuery(command, CommandSender);
            else QueryProcessor.ProcessGameConsoleQuery(command);
        }

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

        public void PlaceBlood(Vector3 pos, int type = 1, float size = 2f)
        {
            var component = ClassManager;
            var writer = NetworkWriterPool.GetWriter();
            writer.WriteVector3(pos);
            writer.WriteInt32(type);
            writer.WriteSingle(size);
            var msg = new RpcMessage
            {
                netId = component.netId,
                componentIndex = component.ComponentIndex,
                functionHash = typeof(CharacterClassManager).FullName.GetStableHashCode() * 503 + "RpcPlaceBlood".GetStableHashCode(),
                payload = writer.ToArraySegment()
            };
            Connection.Send(msg);
            NetworkWriterPool.Recycle(writer);
        }

        private float delay = Time.time;
        private int pos = 0;

        private void Update()
        {
            if (Hub.isDedicatedServer)
                Server.Get.Events.Server.InvokeUpdateEvent();

            if (this == Server.Get.Host || HideRank || SynapseGroup.Color.ToUpper() != "RAINBOW") return;

            if (!string.IsNullOrEmpty(ServerRoles.NetworkGlobalBadge)) return;

            if (Time.time >= delay)
            {
                delay = Time.time + 1f;

                RankColor = Server.Get.Colors.ElementAt(pos);

                pos = pos + 1 >= Server.Get.Colors.Count() ? 0 : pos + 1;
            }
        }
        #endregion

        #region Synapse Api Stuff
        public readonly Jail Jail;

        public readonly Scp106Controller Scp106Controller;

        public readonly Scp079Controller Scp079Controller;

        public readonly Scp096Controller Scp096Controller;

        public readonly Scp173Controller Scp173Controller;

        public BroadcastList ActiveBroadcasts { get; }

        public PlayerInventory Inventory { get; }

        public Broadcast SendBroadcast(ushort time, string message, bool instant = false)
        {
            if (this == Server.Get.Host)
                Logger.Get.Send($"Broadcast: {message}", ConsoleColor.White);

            var bc = new Broadcast(message, time, this);
            ActiveBroadcasts.Add(bc, instant);
            return bc;
        }

        private IRole _role;

        public IRole CustomRole
        {
            get => _role;
            set
            {
                OldRoleID = RoleID;

                if (_role != null)
                    _role.DeSpawn();

                _role = value;

                if (_role == null) return;

                _role.Player = this;
                _role.Spawn();
            }
        }

        public int OldRoleID { get; private set; } = -1;

        public int RoleID
        {
            get
            {
                if (CustomRole == null) return (int)RoleType;
                else return CustomRole.GetRoleID();
            }
            set
            {
                if (value >= 0 && value <= RoleManager.HighestRole)
                {
                    CustomRole = null;
                    RoleType = (RoleType)value;
                    return;
                }

                if (!Server.Get.RoleManager.IsIDRegistered(value))
                    throw new Exception("Plugin tried to set the RoleId of a Player with an not registered RoldeID");

                CustomRole = Server.Get.RoleManager.GetCustomRole(value);
            }
        }

        public string RoleName => CustomRole == null ? RoleType.ToString() : CustomRole.GetRoleName();

        /// <summary>
        /// This field is just for storing some setclass information between multiple Harmony Patches
        /// </summary>
        internal PlayerSetClassEventArgs setClassEventArgs;

        //Stuff for the Permission System
        private SynapseGroup synapseGroup;

        public SynapseGroup SynapseGroup
        {
            get
            {
                if (synapseGroup == null)
                    return Server.Get.PermissionHandler.GetPlayerGroup(this);

                return synapseGroup;
            }
            set
            {
                if (value == null)
                    return;

                synapseGroup = value;

                RefreshPermission(HideRank);
            }
        }

        public bool HasPermission(string permission) => this == Server.Get.Host || SynapseGroup.HasPermission(permission);

        public void RefreshPermission(bool disp)
        {
            var group = new UserGroup
            {
                BadgeText = SynapseGroup.Badge.ToUpper() == "NONE" ? null : SynapseGroup.Badge,
                BadgeColor = SynapseGroup.Color.ToUpper() == "NONE" ? null : SynapseGroup.Color,
                Cover = SynapseGroup.Cover,
                HiddenByDefault = SynapseGroup.Hidden,
                KickPower = SynapseGroup.KickPower,
                Permissions = SynapseGroup.GetVanillaPermissionValue(),
                RequiredKickPower = SynapseGroup.RequiredKickPower,
                Shared = false
            };

            var globalAccesAllowed = true;
            switch (ServerRoles.GlobalBadgeType)
            {
                case 1: globalAccesAllowed = Server.Get.PermissionHandler.serverSection.StaffAccess; break;
                case 2: globalAccesAllowed = Server.Get.PermissionHandler.serverSection.ManagerAccess; break;
                case 3: globalAccesAllowed = Server.Get.PermissionHandler.serverSection.GlobalBanTeamAccess; break;
                case 4: globalAccesAllowed = Server.Get.PermissionHandler.serverSection.GlobalBanTeamAccess; break;
            }
            if (GlobalPerms != 0 && globalAccesAllowed)
                group.Permissions |= GlobalPerms;

            ServerRoles.Group = group;
            ServerRoles.Permissions = group.Permissions;
            RemoteAdminAccess = SynapseGroup.RemoteAdmin || GlobalRemoteAdmin;
            ServerRoles.AdminChatPerms = PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.AdminChat);
            ServerRoles._badgeCover = group.Cover;
            QueryProcessor.GameplayData = PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.GameplayData);

            //Since OverwatchPermitted is a seperate vanilla Central Server Permission it is only activated and never deactivated
            if (!ServerRoles.OverwatchPermitted && PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.AdminChat))
                ServerRoles.OverwatchPermitted = true;

            ServerRoles.SendRealIds();

            if (string.IsNullOrEmpty(group.BadgeText))
            {
                ServerRoles.SetColor(null);
                ServerRoles.SetText(null);
                if (!string.IsNullOrEmpty(ServerRoles.PrevBadge))
                {
                    ServerRoles.HiddenBadge = ServerRoles.PrevBadge;
                    ServerRoles.GlobalHidden = true;
                    ServerRoles.RefreshHiddenTag();
                }
            }
            else
            {
                if (ServerRoles._hideLocalBadge || (group.HiddenByDefault && !disp && !ServerRoles._neverHideLocalBadge))
                {
                    ServerRoles.Network_myText = null;
                    ServerRoles.Network_myColor = "default";
                    ServerRoles.HiddenBadge = group.BadgeText;
                    ServerRoles.RefreshHiddenTag();
                    ServerRoles.TargetSetHiddenRole(Connection, group.BadgeText);
                }
                else
                {
                    ServerRoles.HiddenBadge = null;
                    ServerRoles.RpcResetFixed();
                    ServerRoles.Network_myText = group.BadgeText;
                    ServerRoles.Network_myColor = group.BadgeColor;
                }
            }

            var flag = ServerRoles.Staff || PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.ViewHiddenBadges);
            var flag2 = ServerRoles.Staff || PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.ViewHiddenGlobalBadges);

            if (flag || flag2)
                foreach (var player in Server.Get.Players)
                {
                    if (!string.IsNullOrEmpty(player.ServerRoles.HiddenBadge) && (!player.ServerRoles.GlobalHidden || flag2) && (player.ServerRoles.GlobalHidden || flag))
                        player.ServerRoles.TargetSetHiddenRole(Connection, player.ServerRoles.HiddenBadge);
                }
        }

        public ulong GlobalPerms => ServerRoles._globalPerms;

        public bool GlobalRemoteAdmin => ServerRoles.RemoteAdminMode == ServerRoles.AccessMode.GlobalAccess;

        public bool IsDummy { get; internal set; } = false;
        #endregion

        #region Default Stuff
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

        public bool RemoteAdminAccess
        {
            get => ServerRoles.RemoteAdmin;
            set
            {
                if (value)
                    RaLogin();
                else
                    RaLogout();
            }
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
            set => PlayerMovementSync.OverridePosition(value, 0f);
        }

        public Vector2 Rotation
        {
            get => PlayerMovementSync.RotationSync;
            set => PlayerMovementSync.NetworkRotationSync = value;
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
            get => (PlayerMovementState)AnimationController.Network_curMoveState;
            set => AnimationController.Network_curMoveState = (byte)value;
        }

        public Vector3 Scale
        {
            get => transform.localScale;
            set
            {
                try
                {
                    transform.localScale = value;

                    var method = typeof(NetworkServer).GetMethod("SendSpawnMessage", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

                    foreach (var ply in Server.Get.Players)
                        method.Invoke(null, new object[] { NetworkIdentity, ply.Connection });
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
            get => PlayerStats.GetAhpValue();
            set => PlayerStats.SafeSetAhpValue(value);
        }

        public int MaxArtificialHealth
        {
            get => PlayerStats.MaxArtificialHealth;
            set => PlayerStats.MaxArtificialHealth = value;
        }

        public float Stamina
        {
            get => Hub.fpc.staminaController.RemainingStamina * 100;
            set => Hub.fpc.staminaController.RemainingStamina = (value / 100f);
        }

        public float StaminaUsage
        {
            get => Hub.fpc.staminaController.StaminaUse * 100;
            set => Hub.fpc.staminaController.StaminaUse = (value / 100f);
        }

        public RoleType RoleType
        {
            get => ClassManager.CurClass;
            set
            {
                if (CustomRole == null)
                    OldRoleID = (int)RoleType;

                ClassManager.SetPlayersClass(value, gameObject);
            }
        }

        public Room Room
        {
            get => RoomIdUtils.RoomAtPosition(Position).GetSynapseRoom();
            set => Position = value.Position;
        }

        public MapPoint MapPoint
        {
            get => new MapPoint(Room, Position);
            set => Position = value.Position;
        }

        public Player CurrentlySpectating
        {
            get => SpectatorManager.CurrentSpectatedPlayer?.GetPlayer();
            set => SpectatorManager.CurrentSpectatedPlayer = value.Hub;
        }

        public Player Cuffer
        {
            get
            {
                if (!DisarmedPlayers.Entries.Any(x => x.DisarmedPlayer == NetworkIdentity.netId)) return null;

                var id = DisarmedPlayers.Entries.FirstOrDefault(x => x.DisarmedPlayer == NetworkIdentity.netId).Disarmer;
                if (id == 0)
                    return ReferenceHub.LocalHub.GetPlayer();
                return Server.Get.Players.FirstOrDefault(x => x.NetworkIdentity.netId == id);
            }
            set => VanillaInventory.SetDisarmedStatus(value.VanillaInventory);
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

        [Obsolete("Please use player.AmmoBox[AmmoType.Ammo556x45]",false)]
        public uint Ammo5
        {
            get => AmmoBox[AmmoType.Ammo556x45];
            set => AmmoBox[AmmoType.Ammo556x45] = (ushort)value;
        }

        [Obsolete("Please use player.AmmoBox[AmmoType.Ammo762x39]", false)]
        public uint Ammo7
        {
            get => AmmoBox[AmmoType.Ammo762x39];
            set => AmmoBox[AmmoType.Ammo762x39] = (ushort)value;
        }

        [Obsolete("Please use player.AmmoBox[AmmoType.Ammo9x19]", false)]
        public uint Ammo9
        {
            get => AmmoBox[AmmoType.Ammo9x19];
            set => AmmoBox[AmmoType.Ammo9x19] = (ushort)value;
        }

        public PlayerAmmoBox AmmoBox { get; }

        public class PlayerAmmoBox
        {
            private readonly Player player;
            internal PlayerAmmoBox(Player _player) => player = _player;

            public ushort this[AmmoType ammo]
            {
                get => player.VanillaInventory.UserInventory.ReserveAmmo[(ItemType)ammo];
                set => player.VanillaInventory.UserInventory.ReserveAmmo[(ItemType)ammo] = value;
            }
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
            get => !string.IsNullOrEmpty(ServerRoles.HiddenBadge);
            set
            {
                if (value)
                    ClassManager.CmdRequestHideTag();
                else
                    ClassManager.UserCode_CmdRequestShowTag(false);
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

        public ZoneType Zone => Room.Zone;

        public bool DoNotTrack => ServerRoles.DoNotTrack;

        public bool IsDead => Team == Team.RIP;

        /*
        //TODO: ReImplement this
        public bool IsZooming => false;

        public bool IsReloading => false;
        */

        public bool IsCuffed => DisarmedPlayers.IsDisarmed(VanillaInventory);

        public float AliveTime => ClassManager.AliveTime;

        public string AuthToken => ClassManager.AuthToken;

        public int Ping => LiteNetLib4MirrorServer.Peers[Connection.connectionId].Ping;

        public string NickName => NicknameSync.Network_myNickSync;

        public Team Team => ClassManager.CurRole.team;

        public int TeamID => CustomRole == null ? (int)Team : CustomRole.GetTeamID();

        public Team RealTeam => Server.Get.TeamManager.IsDefaultID(TeamID) ? (Team)TeamID : Team.RIP;

        public Faction Faction => ClassManager.Faction;

        public Faction RealFraction => Misc.GetFaction(RealTeam);

        public SynapseItem ItemInHand
        {
            get
            {
                if (VanillaInventory.CurItem == ItemIdentifier.None) return SynapseItem.None;

                return VanillaInventory.CurInstance.GetSynapseItem();
            }
            set
            {
                if(value == null || value == SynapseItem.None || !Inventory.Items.Contains(value))
                {
                    VanillaInventory.NetworkCurItem = ItemIdentifier.None;
                    VanillaInventory.CurInstance = null;
                }

                if (!ItemInHand.ItemBase.CanHolster() || !value.ItemBase.CanEquip()) return;

                VanillaInventory.NetworkCurItem = new ItemIdentifier(value.ItemType, value.Serial);
                VanillaInventory.CurInstance = value.ItemBase;
            }
        }

        public NetworkConnection Connection => ClassManager.Connection;

        public string IpAddress => QueryProcessor._ipAddress;
        #endregion

        #region ReferenceHub
        public Transform CameraReference => Hub.PlayerCameraReference;

        public NetworkIdentity NetworkIdentity => Hub.networkIdentity;

        public Assets._Scripts.Dissonance.DissonanceUserSetup DissonanceUserSetup { get; }

        public Radio Radio { get; }


        public GameConsoleTransmission GameConsoleTransmission { get; }

        public Escape Escape { get; }

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

        public void TriggerEscape()
        {
            if (CustomRole == null)
            {
                var newRole = -1;
                var allow = true;
                var changeTeam = false;


                foreach(var entry in DisarmedPlayers.Entries)
                    if(entry.DisarmedPlayer == NetworkIdentity.netId)
                    {
                        if(entry.Disarmer == 0)
                        {
                            changeTeam = CharacterClassManager.ForceCuffedChangeTeam;
                            break;
                        }

                        var cuffer = Cuffer;

                        if (RoleType == RoleType.Scientist && cuffer.Faction == Faction.FoundationEnemy)
                            changeTeam = true;
                        else if (RoleType == RoleType.ClassD && cuffer.Faction == Faction.FoundationStaff)
                            changeTeam = true;
                    }

                

                switch (RoleType)
                {
                    case RoleType.ClassD when changeTeam:
                        newRole = (int)RoleType.NtfPrivate;
                        break;

                    case RoleType.ClassD:
                    case RoleType.Scientist when changeTeam:
                        newRole = (int)RoleType.ChaosConscript;
                        break;

                    case RoleType.Scientist:
                        newRole = (int)RoleType.NtfSpecialist;
                        break;
                }

                if (newRole < 0) allow = false;

                var isClassD = RoleID == (int)RoleType.ClassD;

                Server.Get.Events.Player.InvokePlayerEscapeEvent(this, ref newRole, ref isClassD, ref changeTeam, ref allow);

                if (newRole < 0 || !allow) return;

                if (newRole >= -1 && newRole <= RoleManager.HighestRole)
                    ClassManager.SetPlayersClass((RoleType)newRole, gameObject, CharacterClassManager.SpawnReason.Escaped, false);
                else
                    RoleID = newRole;

                Escape.TargetShowEscapeMessage(Connection, isClassD, changeTeam);

                var tickets = Respawning.RespawnTickets.Singleton;
                switch (RealTeam)
                {
                    case Team.MTF when changeTeam:
                        RoundSummary.escaped_scientists++;
                        tickets.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox,
                            GameCore.ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_classd_cuffed_count", 1), false);
                        break;

                    case Team.MTF when !changeTeam:
                        RoundSummary.escaped_scientists++;
                        tickets.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox,
                            GameCore.ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_scientist_count", 1), false);
                        break;

                    case Team.CHI when changeTeam:
                        RoundSummary.escaped_ds++;
                        tickets.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox,
                            GameCore.ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_scientist_cuffed_count", 1), false);
                        break;

                    case Team.CHI when !changeTeam:
                        RoundSummary.escaped_ds++;
                        tickets.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox,
                            GameCore.ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_classd_count", 1), false);
                        break;
                }
            }
            else
            {
                CustomRole.Escape();
                return;
            }
        }
    }
}
