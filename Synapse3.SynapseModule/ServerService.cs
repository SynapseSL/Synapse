using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core.Meta;
using Neuron.Core.Plugins;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using Console = GameCore.Console;

namespace Synapse3.SynapseModule;

public class ServerService : Service
{
    private readonly ServerEvents _server;
    private readonly PluginManager _plugin;

    public ServerService(ServerEvents server, PluginManager plugin)
    {
        _server = server;
        _plugin = plugin;
    }

    public ReadOnlyCollection<PluginContext> Plugins => _plugin.Plugins.AsReadOnly();

    /// <summary>
    /// Configures the ServerName that will be displayed on the Server List
    /// </summary>
    public string Name
    { 
        get => ServerConsole._serverName;
        set
        {
            ServerConsole._serverName = value;
            ServerConsole.RefreshServerName();
        }
    }
    
    /// <summary>
    /// Configures the Port the Server is running on (not recommended to change)
    /// </summary>
    public ushort Port
    {
        get => ServerStatic.ServerPort;
        set => ServerStatic.ServerPort = value;
    }

    /// <summary>
    /// Configures the amount of players that can join the Server
    /// </summary>
    public int Slots
    {
        get => CustomNetworkManager.slots;
        set => CustomNetworkManager.slots = value;
    }
    
    /// <summary>
    /// Configures FriendlyFire
    /// </summary>
    public bool FF
    {
        get => ServerConsole.FriendlyFire;
        set {
            ServerConsole.FriendlyFire = value;
            ServerConfigSynchronizer.RefreshAllConfigs();
        }
    }

    /// <summary>
    /// Returns an array of all Colors that are allowed to use in Badges
    /// </summary>
    public Dictionary<Misc.PlayerInfoColorTypes, string> Colors { get; } = Misc.AllowedColors;

    /// <summary>
    /// Rank badges needs an exact string for each color and therefore returns this a version of the colors that will actually be displayed
    /// </summary>
    public List<string> ValidatedBadgeColors { get; } = Misc.AllowedColors.Keys.Select(x => x switch
    {
        Misc.PlayerInfoColorTypes.LightGreen => "light_green",
        Misc.PlayerInfoColorTypes.DeepPink => "deep_pink",
        Misc.PlayerInfoColorTypes.BlueGreen => "blue_green",
        Misc.PlayerInfoColorTypes.ArmyGreen => "army_green",
        _ => x.ToString().ToLower(),
    }).ToList();

    /// <summary>
    /// Returns the ServerConsole Object from the base game
    /// </summary>
    public ServerConsole ServerConsole => ServerConsole.singleton;

    /// <summary>
    /// Returns the GameConsole Object from the base game
    /// </summary>
    public Console GameConsole => Console.singleton;
    
    /// <summary>
    ///     Bans a player that is not on the server
    /// </summary>
    /// <param name="reason">The reason for the ban</param>
    /// <param name="issuer">The person/SCP that banned the player</param>
    /// <param name="id">The person account id (e.g. xxxxxxxxxxx@steam) to ban</param>
    /// <param name="duration">The duration for the ban  in seconds</param>
    public void OfflineBanID(string reason, string issuer, string id, int duration)
    {
        BanHandler.IssueBan(new BanDetails
        {
            Reason = reason,
            Issuer = issuer,
            Id = id,
            OriginalName = "Unknown - offline ban",
            IssuanceTime = DateTime.UtcNow.Ticks,
            Expires = DateTime.UtcNow.AddSeconds(duration).Ticks
        }, BanHandler.BanType.UserId);
    }

    /// <summary>
    /// Bans a IP
    /// </summary>
    /// <param name="reason">The reason for the ban</param>
    /// <param name="issuer">The person/SCP that banned the player</param>
    /// <param name="ip">The IPv4 or IPv6 to ban</param>
    /// <param name="duration">The duration for the ban in seconds</param>
    public void OfflineBanIP(string reason, string issuer, string ip, int duration)
    {
        BanHandler.IssueBan(new BanDetails
        {
            Reason = reason,
            Issuer = issuer,
            Id = ip,
            OriginalName = "Unknown - offline ban",
            IssuanceTime = DateTime.UtcNow.Ticks,
            Expires = DateTime.UtcNow.AddSeconds(duration).Ticks
        }, BanHandler.BanType.IP);
    }

    /// <summary>
    /// Reloads all Services of Synapse
    /// </summary>
    public void Reload()
    {
        _server.Reload.Raise(new ReloadEvent());
    }
}