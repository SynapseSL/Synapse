using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Database;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Permissions;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer : MonoBehaviour
{
    private readonly PlayerService _player;
    private readonly ServerService _server;
    private readonly CassieService _cassie;
    private readonly PermissionService _permission;
    private readonly RoleService _role;
    private readonly TeamService _team;
    private readonly RoomService _room;
    private readonly PlayerEvents _playerEvents;
    private readonly SynapseConfigService _config;
    private readonly DatabaseService _database;
    private readonly MirrorService _mirror;

    /// <summary>
    /// The Type of Player this is. It can be a normal Player the Server itself or a Dummy
    /// </summary>
    public virtual PlayerType PlayerType => PlayerType.Player;

    internal SynapsePlayer()
    {
        Hub = GetComponent<ReferenceHub>();
        GameConsoleTransmission = GetComponent<GameConsoleTransmission>();
        BroadcastController = GetComponent<global::Broadcast>();

        _player = Synapse.Get<PlayerService>();
        _server = Synapse.Get<ServerService>();
        _cassie = Synapse.Get<CassieService>();
        _permission = Synapse.Get<PermissionService>();
        _role = Synapse.Get<RoleService>();
        _team = Synapse.Get<TeamService>();
        _room = Synapse.Get<RoomService>();
        _playerEvents = Synapse.Get<PlayerEvents>();
        _config = Synapse.Get<SynapseConfigService>();
        _database = Synapse.Get<DatabaseService>();
        _mirror = Synapse.Get<MirrorService>();

        Inventory = new ItemInventory(this);
        ActiveBroadcasts = new BroadcastList(this);
        ActiveHint = new TextHintList(this);
        MainScpController = new ScpController.MainScpController(this, _config);
        CustomInfo = new CustomInfoList(this, _player, _playerEvents);
        FakeRoleManager = new(this, _mirror, _player);
    }
}