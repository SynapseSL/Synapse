using System;
using System.Collections.Generic;
using Mirror;

namespace Synapse3.SynapseModule.Player;

public class FakeRoleManager : IJoinUpdate
{
    private readonly SynapsePlayer _player;
    private readonly MirrorService _mirror;
    private readonly PlayerService _playerService;
    private readonly Dictionary<SynapsePlayer, RoleType> _sendRoles = new();

    public FakeRoleManager(SynapsePlayer player, MirrorService mirror, PlayerService playerService)
    {
        _player = player;
        _mirror = mirror;
        _playerService = playerService;
        _playerService.JoinUpdates.Add(this);
    }

    public void UpdateAll()
    {
        foreach (var player in _playerService.Players)
        {
            UpdatePlayer(player);
        }
    }
    
    public bool NeedsJoinUpdate => true;
    public void UpdatePlayer(SynapsePlayer player)
    {
        var role = _player.RoleType;
        if (player == _player)
        {
            if (OwnVisibleRole != RoleType.None)
                role = OwnVisibleRole;
        }
        else if (ToPlayerVisibleRole.ContainsKey(player))
        {
            role = ToPlayerVisibleRole[player];
        }
        else
        {
            var foundRole = false;
            foreach (var condition in VisibleRoleCondition)
            {
                if (condition.Key.Invoke(player))
                {
                    foundRole = true;
                    role = condition.Value;
                    break;
                }
            }

            if (!foundRole && VisibleRole != RoleType.None)
            {
                role = VisibleRole;
            }
        }
        
        //This will prevent to send unnecessary packages from being send
        if(_sendRoles.ContainsKey(player) && _sendRoles[player] == role)
            return;
        _sendRoles[player] = role;

        player.SendNetworkMessage(_mirror.GetCustomVarMessage(_player.ClassManager, writer =>
        {
            writer.WriteUInt64(8ul);
            writer.WriteSByte((sbyte)role);
        }));
    }

    private RoleType _ownVisibleRole = RoleType.None;

    public RoleType OwnVisibleRole
    {
        get => _ownVisibleRole;
        set
        {
            if (value == _ownVisibleRole) return;
            _ownVisibleRole = value;
            UpdatePlayer(_player);
        }
    }

    private RoleType _visibleRole = RoleType.None;

    public RoleType VisibleRole
    {
        get => _visibleRole;
        set
        {
            if (value == _visibleRole) return;
            _visibleRole = value;
            foreach (var player in _playerService.Players)
            {
                if (player != _player)
                    UpdatePlayer(player);
            }
        }
    }
    public Dictionary<Func<SynapsePlayer, bool>, RoleType> VisibleRoleCondition { get; set; } = new();
    public Dictionary<SynapsePlayer, RoleType> ToPlayerVisibleRole { get; set; } = new();
}