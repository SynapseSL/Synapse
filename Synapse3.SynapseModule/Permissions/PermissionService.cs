using System;
using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Modules.Configs;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Permissions;

public class PermissionService : Service
{
    private uint _currentGroupId = 500;
    private ConfigService _configService;
    private ServerEvents _server;
    public ConfigContainer Container { get; private set; }
    public Dictionary<string, SynapseGroup> Groups { get; private set; } = new ();

    private readonly SynapseGroup _fallBackDefault = new()
    {
        Default = true,
        Permissions = new List<string> { "synapse.command.help", "synapse.command.plugins" }
    };

    public PermissionService(ConfigService configService,ServerEvents server)
    {
        _server = server;
        _configService = configService;
    }

    public override void Enable()
    {
        _server.Reload.Subscribe(Reload);
        try
        {
            Container = _configService.GetContainer("permissions.syml");
            LoadGroups();
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Permission: Failed to load permission.syml\n" + ex);
        }
    }

    public override void Disable()
    {
        _server.Reload.Unsubscribe(Reload);
    }

    public void Reload(ReloadEvent _ = null)
    {
        Container.Load();
        LoadGroups();
    }

    public void Store()
    {
        foreach (var pair in Groups)
        {
            Container.Document.Set(pair.Key, pair.Value);
        }
        
        Container.Store();
        Reload();
    }

    private SynapseGroup GetGroupInsensitive(string key) => Groups
        .FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value;

    private void LoadGroups()
    {
        var groups = new Dictionary<string, SynapseGroup>();
        foreach (var section in Container.Document.Sections)
        {
            try
            {
                if (groups.ContainsKey(section.Key))
                {
                    NeuronLogger.For<SynapseGroup>()
                        .Warn(
                            $"Sy3 Permission: Group {section.Key} was found a second time. Second instance will be skipped");
                    continue;
                }

                var group = section.Value.Export<SynapseGroup>();
                groups[section.Key] = group;
            }
            catch (Exception ex)
            {
                NeuronLogger.For<Synapse>()
                    .Error($"Sy3 Permission: Loading of Permissions section {section.Key} failed\n" + ex);
            }
        }

        Groups = groups;

        if (Groups.Count == 0)
        {
            Groups["Owner"] = new SynapseGroup
            {
                Badge = "Owner",
                Color = "rainbow",
                Cover = true,
                Hidden = true,
                KickPower = 254,
                Members = new List<string> {"0000000@steam"},
                Inheritance = new List<string> {"User"},
                Permissions = new List<string> {"*"},
                RemoteAdmin = true,
                RequiredKickPower = 255
            };

            Groups["User"] = new SynapseGroup
            {
                Default = true,
                Permissions = new List<string> {"synapse.command.help", "synapse.command.plugins"},
                Members = null,
                Inheritance = null,
            };

            Store();
        }


        _currentGroupId = 500;
        foreach (var group in Groups)
        {
            group.Value.GroupId = _currentGroupId++;
        }
        
        
        foreach (var player in Synapse.Get<PlayerService>().Players)
            player.RefreshPermission(player.HideRank);
    }

    public SynapseGroup GetDefaultGroup() => Groups.Values.FirstOrDefault(x => x.Default) ?? _fallBackDefault;
    public SynapseGroup GetNorthwoodGroup() => Groups.Values.FirstOrDefault(x => x.NorthWood)?.Copy();

    public bool AddServerGroup(SynapseGroup group, string groupName)
    {
        var current = GetGroupInsensitive(groupName);
        if (current != null) return false;
        Groups[groupName] = group;
        Store();
        return true;
    }

    public bool DeleteServerGroup(string groupName)
    {
        var removed = Groups.Remove(groupName);
        Store();
        return removed;
    }

    public bool ModifyServerGroup(string groupName, SynapseGroup group)
    {
        var current = GetGroupInsensitive(groupName);
        if (current == null) return false;
        Groups[groupName] = group;
        Store();
        return true;
    }

    public SynapseGroup GetServerGroup(string groupName) => GetGroupInsensitive(groupName);

    public SynapseGroup GetPlayerGroup(SynapsePlayer player)
    {
        var group = Groups.Values.FirstOrDefault(x => x.Members != null && x.Members.Contains(player.UserId));

        if (group != null)
            return group.Copy();

        var nwGroup = GetNorthwoodGroup();

        if (player.ServerRoles.Staff && nwGroup != null)
            return nwGroup;

        return GetDefaultGroup();
    }

    public SynapseGroup GetPlayerGroup(string UserID)
    {
        var group = Groups.Values.FirstOrDefault(x => x.Members != null && x.Members.Contains(UserID));

        if (group != null)
            return group.Copy();

        var nwGroup = GetNorthwoodGroup();

        if (UserID.ToLower().Contains("@northwood") && nwGroup != null)
            return nwGroup;

        return GetDefaultGroup();
    }
    
    public bool AddPlayerToGroup(string groupName, string userid)
    {
        var group = GetGroupInsensitive(groupName);
        if (group == null) return false;
        if (!userid.Contains("@")) return false;

        RemovePlayerGroup(userid);

        group.Members ??= new List<string>();

        group.Members.Add(userid);
        Store();
        return true;
    }

    public bool RemovePlayerGroup(string userid)
    {
        if (!userid.Contains("@")) return false;
        var doSave = false;
        foreach(var group in Groups.Where(x => x.Value.Members != null && x.Value.Members.Contains(userid)))
        {
            group.Value.Members.Remove(userid);
            doSave = true;
        }
        if (doSave) Store();
        return doSave;
    }
}