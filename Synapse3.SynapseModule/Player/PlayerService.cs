using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PluginAPI.Core;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Permissions;
using Synapse3.SynapseModule.Permissions.RemoteAdmin;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public class PlayerService : Service
{
    private readonly DummyService _dummy;
    private readonly PlayerEvents _player;
    private readonly RoundEvents _round;
    private readonly PermissionService _permission;

    public List<IJoinUpdate> JoinUpdates { get; } = new();

    public PlayerService(DummyService dummy, PlayerEvents player, RoundEvents round, PermissionService permission)
    {
        _dummy = dummy;
        _player = player;
        _round = round;
        _permission = permission;
    }

    public override void Enable()
    {
        _player.Join.Subscribe(Join);
        _round.Restart.Subscribe(ClearJoinUpdates);
    }

    public override void Disable()
    {
        _player.Join.Unsubscribe(Join);
        _round.Restart.Unsubscribe(ClearJoinUpdates);
    }

    private void ClearJoinUpdates(RoundRestartEvent _) => JoinUpdates.Clear();

    /// <summary>
    /// Returns the Host Player
    /// </summary>
    public SynapseServerPlayer Host { get; internal set; }
    
    private List<SynapsePlayer> _players = new();
    /// <summary>
    /// Returns a ReadOnly List of all actual Players on the Server
    /// </summary>
    public ReadOnlyCollection<SynapsePlayer> Players => _players.AsReadOnly();

    internal void AddPlayer(SynapsePlayer player) => _players.Add(player);
    internal void RemovePlayer(SynapsePlayer player) => _players.Remove(player);

    /// <summary>
    /// Returns the amount of players on the server
    /// </summary>
    public int PlayersAmount => ServerConsole._playersAmount;

    /// <summary>
    /// Returns all Player objects even the Host and all Dummies
    /// </summary>
    public List<SynapsePlayer> GetAbsoluteAllPlayers() =>
        GetPlayers(PlayerType.Player, PlayerType.Server, PlayerType.Dummy);
    
    public List<SynapsePlayer> GetPlayers(params PlayerType[] playerTypes)
    {
        var result = new List<SynapsePlayer>();
        if (playerTypes.Contains(PlayerType.Player))
        {
            result.AddRange(Players);
        }
        
        if (playerTypes.Contains(PlayerType.Server))
        {
            result.Add(Host);
        }

        if (playerTypes.Contains(PlayerType.Dummy))
        {
            foreach (var dummy in _dummy._dummies)
            {
                result.Add(dummy.Player);
            }
        }

        return result;
    }
    
    public List<SynapsePlayer> GetPlayers(Func<SynapsePlayer, bool> func)
        => GetAbsoluteAllPlayers().Where(func).ToList();

    public List<SynapsePlayer> GetPlayers(Func<SynapsePlayer, bool> func, params PlayerType[] playerTypes)
        => GetPlayers(playerTypes).Where(func).ToList();
    
    public SynapsePlayer GetPlayer(Func<SynapsePlayer, bool> func)
        => GetAbsoluteAllPlayers().FirstOrDefault(func);

    public SynapsePlayer GetPlayer(Func<SynapsePlayer, bool> func, params PlayerType[] playerTypes)
        => GetPlayers(playerTypes).FirstOrDefault(func);


    public SynapsePlayer GetPlayer(string argument) =>
        GetPlayer(argument, PlayerType.Player, PlayerType.Server, PlayerType.Dummy);
    
    /// <summary>
    /// Returns a Player based upon the given argument
    /// </summary>
    /// <param name="argument">UserID, Name, PlayerID or NetID as string</param>
    /// <param name="playerTypes">The Player Types that should be returned</param>
    public SynapsePlayer GetPlayer(string argument, params PlayerType[] playerTypes)
    {
        if (argument.Contains("@"))
        {
            var player = GetPlayerByUserId(argument, playerTypes);
            if (player != null)
                return player;
        }

        if (int.TryParse(argument, out var playerId))
        {
            var player = GetPlayer(playerId, playerTypes);
            if (player != null)
                return player;
        }
        
        if (uint.TryParse(argument, out var netId))
        {
            var player = GetPlayer(netId, playerTypes);
            if (player != null)
                return player;
        }

        return GetPlayerByName(argument, playerTypes);
    }

    /// <summary>
    /// Returns the player with that playerID
    /// </summary>
    public SynapsePlayer GetPlayer(int playerId)
        => GetPlayer(x => x.PlayerId == playerId);
    
    /// <summary>
    /// Returns the player with that playerID
    /// </summary>
    public SynapsePlayer GetPlayer(int playerId, params PlayerType[] playerTypes)
        => GetPlayer(x => x.PlayerId == playerId,playerTypes);

    /// <summary>
    /// Returns the player with that NetworkID
    /// </summary>
    public SynapsePlayer GetPlayer(uint netId)
        => GetPlayer(x => x.NetworkIdentity.netId == netId);

    /// <summary>
    /// Returns the player with that NetworkID
    /// </summary>
    public SynapsePlayer GetPlayer(uint netId, params PlayerType[] playerTypes)
        => GetPlayer(x => x.NetworkIdentity.netId == netId, playerTypes);

    /// <summary>
    /// Returns the player with that UserID
    /// </summary>
    public SynapsePlayer GetPlayerByUserId(string userid)
        => GetPlayer(x => x.UserId == userid || x.SecondUserID == userid);

    /// <summary>
    /// Returns the player with that UserID
    /// </summary>
    public SynapsePlayer GetPlayerByUserId(string userid, params PlayerType[] playerTypes)
        => GetPlayer(x => x.UserId == userid || x.SecondUserID == userid, playerTypes);

    /// <summary>
    /// Returns the player with that Name
    /// </summary>
    public SynapsePlayer GetPlayerByName(string name)
        => GetPlayer(x =>
            string.Equals(x.DisplayName, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.NickName, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns the player with that Name
    /// </summary>
    public SynapsePlayer GetPlayerByName(string name, params PlayerType[] playerTypes)
        => GetPlayer(x =>
            string.Equals(x.DisplayName, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.NickName, name, StringComparison.OrdinalIgnoreCase), playerTypes);

    public List<SynapsePlayer> GetPlayers(int synapseGroupId)
        => GetPlayers(x => x.SynapseGroup.GroupId == synapseGroupId);

    public List<SynapsePlayer> GetPlayers(int synapseGroupId, params PlayerType[] playerTypes)
        => GetPlayers(x => x.SynapseGroup.GroupId == synapseGroupId, playerTypes);

    /// <summary>
    /// Returns multiple Player that are parsed from a string.
    /// Use . between each player
    /// </summary>
    /// <param name="arg">The Argument that should be parsed to a player list</param>
    /// <param name="players">The Player List that will be returned</param>
    /// <param name="me">The Player which should be returned for Me and Self</param>
    /// <param name="playerTypes">The Player Types which can be returned</param>
    public bool TryGetPlayers(string arg, out HashSet<SynapsePlayer> players, SynapsePlayer me = null) =>
        TryGetPlayers(arg, out players, me, PlayerType.Player, PlayerType.Dummy);
    
    /// <summary>
    /// Returns multiple Player that are parsed from a string.
    /// Use . between each player
    /// </summary>
    /// <param name="arg">The Argument that should be parsed to a player list</param>
    /// <param name="players">The Player List that will be returned</param>
    /// <param name="me">The Player which should be returned for Me and Self</param>
    /// <param name="playerTypes">The Player Types which can be returned</param>
    public bool TryGetPlayers(string arg, out HashSet<SynapsePlayer> players, SynapsePlayer me = null, params PlayerType[] playerTypes)
    {
        var service = Synapse.Get<RemoteAdminCategoryService>();
        players = new HashSet<SynapsePlayer>();
        var all = GetPlayers(playerTypes);
        var args = arg.Split('.');

        foreach (var parameter in args)
        {
            if(string.IsNullOrWhiteSpace(parameter)) continue;
            switch (parameter.ToUpper())
            {
                case "SELF":
                case "ME":
                    if (me == null) continue;
                    players.Add(me);
                    continue;

                case "RA":
                case "REMOTEADMIN":
                case "ADMIN":
                case "STAFF":
                    foreach (var player in all)
                        if (player.ServerRoles.RemoteAdmin)
                            players.Add(player);
                    continue;

                case "NW":
                case "NORTHWOOD":
                case "GLOBALSTAFF":
                    foreach (var player in all)
                        if (player.ServerRoles.Staff)
                            players.Add(player);
                    break;

                case "DM":
                case "NPC":
                case "DUMMY":
                    NeuronLogger.For<Synapse>().Warn("parameter: " + parameter.ToUpper());
                    foreach (var player in all)
                        if (player is DummyPlayer dummy && dummy.RaVisible)
                            players.Add(player);
                    break;

                case "DE":
                case "DEFAULT":
                    foreach (var player in Players)
                    {
                        if (!_permission.Groups.Values.Contains(player.SynapseGroup))
                            players.Add(player);
                    }
                    break;

                case "*":
                case "ALL":
                case "EVERYONE":
                    foreach (var player in all)
                        players.Add(player);
                    continue;

                default:
                    var player3 = GetPlayer(parameter);

                    if (player3 == null)
                    {
                        if (int.TryParse(parameter, out var id))
                        {
                            //Check For SynapseGroupID
                            foreach (var player in GetPlayers(id, playerTypes))
                            {
                                players.Add(player);
                            }

                            //Check for RemoteAdmin Category
                            var category = service.GetCategory(id);
                            if (category != null)
                            {
                                foreach (var player in category.GetPlayers() ?? new())
                                {
                                    players.Add(player);
                                }
                            }
                        }
                        continue;
                    }

                    players.Add(player3);
                    continue;
            }
        }

        return players.Count > 0;
    }

    private void Join(JoinEvent ev)
    {
        foreach (var joinUpdate in JoinUpdates)
        {
            if (joinUpdate == null) continue;
            
            if (joinUpdate.NeedsJoinUpdate)
                joinUpdate.UpdatePlayer(ev.Player);
        }
    }
}