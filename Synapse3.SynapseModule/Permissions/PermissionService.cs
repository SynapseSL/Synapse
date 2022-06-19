using System;
using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Meta;
using Neuron.Modules.Configs;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Permissions;

public class PermissionService : Service
{
    private ConfigService _configService;
    public ConfigContainer Container { get; set; }
    public Dictionary<string, SynapseGroup> Groups { get; set; }

    private SynapseGroup _fallbackDefault = new SynapseGroup
    {
        Default = true,
        Permissions = new List<string> {"synapse.command.help", "synapse.command.plugins"},
        Members = null,
        Inheritance = null,
    };

    public PermissionService(ConfigService configService)
    {
        _configService = configService;
    }

    public override void Enable()
    {
        Container = _configService.GetContainer("permissions.syml");
        LoadGroups();
    }

    public void Reload()
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

    public SynapseGroup GetGroupInsensitive(string key) => Groups
        .FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value;

    private void LoadGroups()
    {
        Groups = Container.Document.Sections.Select(x => (x.Key, Value: x.Value.Export<SynapseGroup>())).ToDictionary(
            x => x.Key,
            x => x.Value
        );

        if (Groups.Count == 0)
        {
            Groups["Owner"] = new SynapseGroup()
            {
                Badge = "Owner",
                Color = "red",
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
        
        
        foreach (var player in Synapse.Get<PlayerService>().Players)
            player.RefreshPermission(player.HideRank);
        
    }

    public SynapseGroup GetDefaultGroup() => Groups.Values.FirstOrDefault(x => x.Default) ?? _fallbackDefault;
    public SynapseGroup GetNorthwoodGroup() => Groups.Values.FirstOrDefault(x => x.Northwood).Copy();

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

        var nwgroup = GetNorthwoodGroup();

        if (player.ServerRoles.Staff && nwgroup != null)
            return nwgroup;

        return GetDefaultGroup();
    }

    public SynapseGroup GetPlayerGroup(string UserID)
    {
        var group = Groups.Values.FirstOrDefault(x => x.Members != null && x.Members.Contains(UserID));

        if (group != null)
            return group.Copy();

        var nwgroup = GetNorthwoodGroup();

        if (UserID.ToLower().Contains("@northwood") && nwgroup != null)
            return nwgroup;

        return GetDefaultGroup();
    }
    
    public bool AddPlayerToGroup(string groupname, string userid)
    {
        var group = GetGroupInsensitive(groupname);
        if (group == null) return false;
        if (!userid.Contains("@")) return false;

        RemovePlayerGroup(userid);

        if (group.Members == null) group.Members = new List<string>();

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