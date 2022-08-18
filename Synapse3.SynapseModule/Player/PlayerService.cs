using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Permissions.RemoteAdmin;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public class PlayerService : Service
{
    private DummyService _dummy;
    private PlayerEvents _player;

    public PlayerService(DummyService dummy, PlayerEvents player)
    {
        _dummy = dummy;
        _player = player;
    }

    public override void Enable()
    {
        _player.Update.Subscribe(PlayerUpdate);
    }

    public override void Disable()
    {
        _player.Update.Unsubscribe(PlayerUpdate);
    }

    private void PlayerUpdate(UpdateEvent ev)
    {
        if (Vector3.Distance(ev.Player.Position, ev.Player.Escape.worldPosition) < Escape.radius)
            ev.Player.TriggerEscape();
    }

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
    public int PlayersAmount => ServerConsole.PlayersAmount;

    /// <summary>
    /// Returns all Player objects even the Host and all Dummies
    /// </summary>
    public List<SynapsePlayer> GetAbsoluteAllPlayers()
    {
        var list = Players.ToList();
        list.Add(Host);
        foreach (var dummy in _dummy._dummies)
        {
            list.Add(dummy.Player);
        }
        return list;
    }

    /// <summary>
    /// Returns a Player based upon the given argument
    /// </summary>
    /// <param name="argument">UserID, Name, PlayerID or NetID as string</param>
    public SynapsePlayer GetPlayer(string argument)
    {
        if (argument.Contains("@"))
        {
            var player = GetPlayerByUserId(argument);
            if (player != null)
                return player;
        }

        if (int.TryParse(argument, out var playerid))
        {
            var player = GetPlayer(playerid);
            if (player != null)
                return player;
        }
        
        if (uint.TryParse(argument, out var netId))
        {
            var player = GetPlayer(netId);
            if (player != null)
                return player;
        }

        return GetPlayerByName(argument);
    }

    /// <summary>
    /// Returns the player with that playerID
    /// </summary>
    public SynapsePlayer GetPlayer(int playerId)
        => GetPlayer(x => x.PlayerId == playerId);

    /// <summary>
    /// Returns the player with that NetworkID
    /// </summary>
    public SynapsePlayer GetPlayer(uint netId)
        => GetPlayer(x => x.NetworkIdentity.netId == netId);
    
    public SynapsePlayer GetPlayer(Func<SynapsePlayer, bool> func)
        => GetAbsoluteAllPlayers().FirstOrDefault(func);

    /// <summary>
    /// Returns the player with that UserID
    /// </summary>
    public SynapsePlayer GetPlayerByUserId(string userid)
        => GetPlayer(x => x.UserId == userid || x.SecondUserID == userid);

    /// <summary>
    /// Returns the player with that Name
    /// </summary>
    public SynapsePlayer GetPlayerByName(string name)
        => GetPlayer(x =>
            string.Equals(x.DisplayName, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.NickName, name, StringComparison.OrdinalIgnoreCase));

    public List<SynapsePlayer> GetPlayers(int synapseGroupId)
        => GetPlayers(x => x.SynapseGroup.GroupId == synapseGroupId);

    public List<SynapsePlayer> GetPlayers(Func<SynapsePlayer, bool> func)
        => GetAbsoluteAllPlayers().Where(func).ToList();

    /// <summary>
    /// Returns multiple Player that are parsed from a string.
    /// Use . between each player
    /// </summary>
    /// <param name="me">The Player which should be returned for Me and Self</param>
    public bool TryGetPlayers(string arg, out HashSet<SynapsePlayer> players, SynapsePlayer me = null)
    {
        var service = Synapse.Get<RemoteAdminCategoryService>();
        players = new HashSet<SynapsePlayer>();
        var all = GetAbsoluteAllPlayers();
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

                case "REMOTEADMIN":
                case "ADMIN":
                case "STAFF":
                    foreach (var player in all)
                        if (player.ServerRoles.RemoteAdmin)
                            players.Add(player);
                    continue;

                case "NW":
                case "NORTHWOOD":
                    foreach (var player in all)
                        if (player.ServerRoles.Staff)
                            players.Add(player);
                    break;

                case "*":
                case "ALL":
                case "EVERYONE":
                    foreach (var player2 in all)
                        players.Add(player2);
                    continue;

                default:
                    var player3 = GetPlayer(parameter);
                    
                    if (player3 == null)
                    {
                        //Check For SynapseGroupID
                        if (int.TryParse(parameter, out var id))
                        {
                            foreach (var player in GetPlayers(id))
                                players.Add(player);

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
}