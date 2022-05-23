using System;
using System.Collections.Generic;
using System.Linq;
using Synapse.Api;
using Synapse.Config;

namespace Synapse.Permission
{
    public class PermissionHandler
    {
        internal PermissionHandler() { }

        private SYML _permissionSYML;

        internal readonly Dictionary<string, SynapseGroup> groups = new();
        internal ServerSection serverSection;

        public static PermissionHandler Get => Server.Get.PermissionHandler;

        public IDictionary<string, SynapseGroup> Groups => groups;

        internal void Init()
        {
            _permissionSYML = new(Server.Get.Files.PermissionFile);
            Reload();
        }

        public void Reload()
        {
            _permissionSYML.Load();
            serverSection = new();
            serverSection = _permissionSYML.GetOrSetDefault("Server", serverSection);
            groups.Clear();

            foreach (var pair in _permissionSYML.Sections)
                if (pair.Key.Equals("server", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        var group = pair.Value.LoadAs<SynapseGroup>();
                        groups.Add(pair.Key, group);
                    }
                    catch (Exception e)
                    {
                        Logger.Get.Error($"Synapse-Permission: Section {pair.Key} in permission.syml is no SynapseGroup or ServerGroup\n{e}");
                    }
                }

            if (groups.Count == 0)
            {
                SynapseGroup group = new()
                {
                    Badge = "Owner",
                    Color = "red",
                    Cover = true,
                    Hidden = true,
                    KickPower = 254,
                    Members = new List<string> { "0000000@steam" },
                    Inheritance = new List<string> { "User" },
                    Permissions = new List<string> { "*" },
                    RemoteAdmin = true,
                    RequiredKickPower = 255
                };

                AddServerGroup(group, "Owner");

                AddServerGroup(GetDefaultGroup(), "User");
            }

            foreach (var player in Server.Get.Players)
                player.RefreshPermission(player.HideRank);
        }

        public void AddServerGroup(SynapseGroup group, string groupname)
        {
            group = _permissionSYML.GetOrSetDefault(groupname, group);
            groups.Add(groupname, group);
            Reload();
        }

        public bool DeleteServerGroup(string groupname)
        {
            if (groupname.Equals("server", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (!_permissionSYML.Sections.Any(x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)))
                return false;

            _permissionSYML.Sections.Remove(_permissionSYML.Sections.First(x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)).Key);
            _permissionSYML.Store();
            Reload();

            return true;
        }

        public bool ModifyServerGroup(string groupname, SynapseGroup group)
        {
            if (groupname.Equals("server", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (!_permissionSYML.Sections.TryFind(out var v, x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)))
                return false;

            v.Value.Import(group);
            _permissionSYML.Store();
            Reload();

            return true;
        }

        public SynapseGroup GetServerGroup(string groupname)
        {
            if (!groups.TryFind(out var v, x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase))) return null;
            return v.Value.Copy();
        }

        public SynapseGroup GetPlayerGroup(Player player)
        {
            if (groups.Values.TryFind(out var group, x => x.Members != null && x.Members.Contains(player.UserId)))
                return group.Copy();

            var nwgroup = GetNorthwoodGroup();

            if (player.ServerRoles.Staff && nwgroup is not null)
                return nwgroup;

            return GetDefaultGroup();
        }

        public SynapseGroup GetPlayerGroup(string UserID)
        {
            if (groups.Values.TryFind(out var group, x => x.Members is not null && x.Members.Contains(UserID)))
                return group.Copy();

            var nwgroup = GetNorthwoodGroup();

            if (nwgroup is not null && UserID.ToLower().Contains("@northwood"))
                return nwgroup;

            return GetDefaultGroup();
        }

        public SynapseGroup GetDefaultGroup()
        {
            if (groups.Values.TryFind(out var group, x => x.Default))
                return group.Copy();

            return new()
            {
                Default = true,
                Permissions = new List<string> { "synapse.command.help", "synapse.command.plugins" },
                Members = null,
                Inheritance = null,
            };
        }

        public SynapseGroup GetNorthwoodGroup() => groups.Values.FirstOrDefault(x => x.Northwood)?.Copy();

        public bool AddPlayerToGroup(string groupname, string userid)
        {
            var group = GetServerGroup(groupname);

            if (group is null)
            {
                Logger.Get.Warn($"Group {groupname} does not exist!");
                return false;
            }

            if (!userid.Contains("@"))
                return false;

            RemovePlayerGroup(userid);

            if (group.Members is null)
                group.Members = new();

            group.Members.Add(userid);

            if (_permissionSYML.Sections.TryFind(out var _v, x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase))) _v.Value.Import(group);

            _permissionSYML.Store();

            Reload();

            return true;
        }

        public bool RemovePlayerGroup(string userid)
        {
            if (!userid.Contains("@"))
                return false;

            var safe = false;
            foreach (var group in groups)
            {
                if (group.Value.Members is null || !group.Value.Members.Contains(userid)) continue;
                group.Value.Members.Remove(userid);
                _permissionSYML.Sections[group.Key].Import(group.Value);
                safe = true;
            }

            if (safe)
            {
                _permissionSYML.Store();
                Reload();
            }

            return true;
        }
    }
}