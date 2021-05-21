using System;
using System.Linq;
using System.Reflection;
using Hints;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using RemoteAdmin;
using Searching;
using Synapse.Api.Enum;
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
            Scp173Controller = new Scp173Controller(this);
            Jail = new Jail(this);
            ActiveBroadcasts = new BroadcastList(this);
            Inventory = new PlayerInventory(this);
            GrenadeManager = GetComponent<Grenades.GrenadeManager>();
            GameConsoleTransmission = this.GetComponent<GameConsoleTransmission>();
            MicroHID = GetComponent<MicroHID>();
            DissonanceUserSetup = this.GetComponent<Assets._Scripts.Dissonance.DissonanceUserSetup>();
            Radio = this.GetComponent<Radio>();
            Escape = this.GetComponent<Escape>();
        }

        #region Methods
        public int GetSightPreference(ItemType item) => GetPreference(item, 0);

        public int GetBarrelPreference(ItemType item) => GetPreference(item, 1);

        public int GetOtherPreference(ItemType item) => GetPreference(item, 2);

        private int GetPreference(ItemType item, int type)
        {
            for (int i = 0; i < WeaponManager.weapons.Length; i++)
            {
                if (WeaponManager.weapons[i].inventoryID == item)
                    return WeaponManager.modPreferences[i, type];
            }
            return 0;
        }

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
            ServerRoles.TargetOpenRemoteAdmin(Connection, false);
        }

        public void RaLogout()
        {
            Hub.serverRoles.RemoteAdmin = false;
            Hub.serverRoles.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
            Hub.serverRoles.TargetCloseRemoteAdmin(Connection);
        }

        public void Heal(float hp) => PlayerStats.HealHPAmount(hp);

        public void Hurt(int amount, DamageTypes.DamageType damagetype = default, Player attacker = null)
        {
            if (attacker == null) attacker = this;
            attacker.PlayerStats.HurtPlayer(new PlayerStats.HitInfo(amount, attacker == null ? "WORLD" : attacker.NickName, damagetype, attacker == null ? 0 : attacker.PlayerId), gameObject);
        }

        public void OpenReportWindow(string text) => GameConsoleTransmission.SendToClient(Connection, "[REPORTING] " + text, "white");

        public void RemoveDisplayInfo(PlayerInfoArea playerInfo) => NicknameSync.Network_playerInfoToShow &= ~playerInfo;

        public void AddDisplayInfo(PlayerInfoArea playerInfo) => NicknameSync.Network_playerInfoToShow |= playerInfo;

        public void ExecuteCommand(string command, bool RA = true) => GameCore.Console.singleton.TypeCommand(RA ? "/" : "" + command, CommandSender);

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
            writer.WritePackedInt32(type);
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
                if (_role != null)
                    _role.DeSpawn();

                _role = value;

                if (_role == null) return;

                _role.Player = this;
                _role.Spawn();
            }
        }

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

        internal Vector3 spawnPosition;

        internal float spawnRotation;

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
                BadgeColor = SynapseGroup.Color.ToLower(),
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

            if (!ServerRoles.OverwatchPermitted && SynapseGroup.HasVanillaPermission(PlayerPermissions.Overwatch))
                ServerRoles.OverwatchPermitted = true;

            RemoteAdminAccess = SynapseGroup.RemoteAdmin || GlobalRemoteAdmin;

            ServerRoles.SendRealIds();

            var flag = ServerRoles.Staff || SynapseGroup.HasVanillaPermission(PlayerPermissions.ViewHiddenBadges);
            var flag2 = ServerRoles.Staff || SynapseGroup.HasVanillaPermission(PlayerPermissions.ViewHiddenGlobalBadges);

            if (flag || flag2)
                foreach (var player in Server.Get.Players)
                {
                    if (!string.IsNullOrEmpty(player.ServerRoles.HiddenBadge) && (!player.ServerRoles.GlobalHidden || flag2) && (player.ServerRoles.GlobalHidden || flag))
                        player.ServerRoles.TargetSetHiddenRole(Connection, player.ServerRoles.HiddenBadge);
                }

            if (group.BadgeColor == "none")
                return;

            if (ServerRoles._hideLocalBadge || (group.HiddenByDefault && !disp && !ServerRoles._neverHideLocalBadge))
            {
                ServerRoles._badgeCover = false;
                if (!string.IsNullOrEmpty(RankName))
                    return;

                ServerRoles.NetworkMyText = null;
                ServerRoles.NetworkMyColor = "default";
                ServerRoles.HiddenBadge = group.BadgeText;
                ServerRoles.RefreshHiddenTag();
                ServerRoles.TargetSetHiddenRole(Connection, group.BadgeText);
            }
            else
            {
                ServerRoles.HiddenBadge = null;
                ServerRoles.RpcResetFixed();
                ServerRoles.NetworkMyText = group.BadgeText;
                ServerRoles.NetworkMyColor = group.BadgeColor;
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
            get => PlayerStats.unsyncedArtificialHealth;
            set => PlayerStats.unsyncedArtificialHealth = value;
        }

        public int MaxArtificialHealth
        {
            get => PlayerStats.maxArtificialHealth;
            set => PlayerStats.maxArtificialHealth = value;
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
            set => ClassManager.SetPlayersClass(value, gameObject);
        }

        public Room Room
        {
            get
            {
                if (Vector3.Distance(Vector3.up * -1997, Position) <= 50) return Map.Get.GetRoom(RoomInformation.RoomType.POCKET);
                return Map.Get.Rooms.FirstOrDefault(x => x.GameObject == Hub.localCurrentRoomEffects.curRoom);
            }
            set => Position = value.Position;
        }

        public MapPoint MapPoint
        {
            get => new MapPoint(Room, Position);
            set => Position = value.Position;
        }

        internal Inventory.SyncListItemInfo VanillaItems
        {
            get => VanillaInventory.items;
            set => VanillaInventory.items = value;
        }

        public Player Cuffer
        {
            get => Handcuffs.CufferId == -1 && Hub.handcuffs.NetworkForceCuff ? Server.Get.Host : SynapseController.Server.GetPlayer(Handcuffs.CufferId);
            set
            {
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
            get
            {
                try
                {
                    return AmmoBox[0];
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    AmmoBox[0] = value;
                }
                catch
                {
                }
            }
        }

        public uint Ammo7
        {
            get
            {
                try
                {
                    return AmmoBox[1];
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    AmmoBox[1] = value;
                }
                catch
                {
                }
            }
        }

        public uint Ammo9
        {
            get
            {
                try
                {
                    return AmmoBox[2];
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    AmmoBox[2] = value;
                }
                catch
                {
                }
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

        public ZoneType Zone => Room.Zone;

        public bool DoNotTrack => ServerRoles.DoNotTrack;

        public bool IsDead => Team == Team.RIP;

        public bool IsZooming => WeaponManager.ZoomInProgress();

        public bool IsReloading => WeaponManager.IsReloading();

        public bool IsCuffed => Cuffer != null;

        public float AliveTime => ClassManager.AliveTime;

        public string AuthToken => ClassManager.AuthToken;

        public int Ping => LiteNetLib4MirrorServer.Peers[Connection.connectionId].Ping;

        public string NickName => NicknameSync.Network_myNickSync;

        public Team Team => ClassManager.CurRole.team;

        public int TeamID => CustomRole == null ? (int)Team : CustomRole.GetTeamID();

        public Team RealTeam => Server.Get.TeamManager.IsDefaultID(TeamID) ? (Team)TeamID : Team.RIP;

        public Fraction Fraction => ClassManager.Fraction;

        public Fraction RealFraction => Misc.GetFraction(RealTeam);

        public Items.SynapseItem ItemInHand
        {
            get => Map.Get.Items.FirstOrDefault(x => x.State == ItemState.Inventory && x.itemInfo.uniq == VanillaInventory.itemUniq);
            set
            {
                if (value != null && value.ItemHolder != this) return;
                
                for(int i = 0; i < 50; i++)
                {
                    MEC.Timing.CallDelayed(i / 100, () =>
                      {
                          VanillaInventory.NetworkitemUniq = value == null? -1 : value.itemInfo.uniq;
                          VanillaInventory.Network_curItemSynced = value == null? ItemType.None : value.ItemType;
                      });
                }
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

        public MicroHID MicroHID { get; }

        public GameConsoleTransmission GameConsoleTransmission { get; }

        public Grenades.GrenadeManager GrenadeManager { get; }

        public Escape Escape { get; }

        public LocalCurrentRoomEffects LocalCurrentRoomEffects => Hub.localCurrentRoomEffects;

        public WeaponManager WeaponManager => Hub.weaponManager;

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

                if (Handcuffs.ForceCuff && CharacterClassManager.ForceCuffedChangeTeam)
                    changeTeam = true;

                if (IsCuffed && CharacterClassManager.CuffedChangeTeam)
                {
                    switch (RoleID)
                    {
                        case (int)RoleType.Scientist when Cuffer.Fraction == Fraction.FoundationEnemy:
                            changeTeam = true;
                            break;

                        case (int)RoleType.ClassD when Cuffer.Fraction == Fraction.FoundationStaff:
                            changeTeam = true;
                            break;
                    }
                }

                switch (RoleID)
                {
                    case (int)RoleType.ClassD when changeTeam:
                        newRole = (int)RoleType.NtfCadet;
                        break;

                    case (int)RoleType.ClassD:
                    case (int)RoleType.Scientist when changeTeam:
                        newRole = (int)RoleType.ChaosInsurgency;
                        break;

                    case (int)RoleType.Scientist:
                        newRole = (int)RoleType.NtfScientist;
                        break;
                }

                if (newRole < 0) allow = false;

                var isClassD = RoleID == (int)RoleType.ClassD;

                Server.Get.Events.Player.InvokePlayerEscapeEvent(this, ref newRole, ref isClassD, ref changeTeam, ref allow);

                if (newRole < 0 || !allow) return;

                if (newRole >= -1 && newRole <= RoleManager.HighestRole)
                    ClassManager.SetPlayersClass((RoleType)newRole, gameObject, false, true);
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

                    case Team.MTF:
                        RoundSummary.escaped_scientists++;
                        tickets.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox,
                            GameCore.ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_scientist_count", 1), false);
                        break;

                    case Team.CHI when changeTeam:
                        RoundSummary.escaped_ds++;
                        tickets.GrantTickets(Respawning.SpawnableTeamType.NineTailedFox,
                            GameCore.ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_scientist_cuffed_count", 1), false);
                        break;

                    case Team.CHI:
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
