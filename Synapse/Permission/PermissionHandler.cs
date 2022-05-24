using Synapse.Api;
using Synapse.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Permission
{
    public class PermissionHandler
    {
        internal PermissionHandler() { }

        private SYML _permissionSYML;

        internal readonly Dictionary<string, SynapseGroup> groups = new Dictionary<string, SynapseGroup>();
        internal ServerSection ServerSection { get; private set; }

        public static PermissionHandler Get
            => Server.Get.PermissionHandler;

        public Dictionary<string, SynapseGroup> Groups 
            => new Dictionary<string, SynapseGroup>(groups);

        internal void Init()
        {
            _permissionSYML = new SYML(Server.Get.Files.PermissionFile);
            Reload();
        }

        public void Reload()
        {
            _permissionSYML.Load();
            ServerSection = new ServerSection();
            ServerSection = _permissionSYML.GetOrSetDefault("Server", ServerSection);
            groups.Clear();

            foreach (var pair in _permissionSYML.Sections)
            {
                if (!pair.Key.Equals("server", StringComparison.InvariantCultureIgnoreCase))
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
            }

            if (groups.Count == 0)
            {
                var group = new SynapseGroup()
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

            _ = _permissionSYML.Sections.Remove(_permissionSYML.Sections.First(x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)).Key);
            _permissionSYML.Store();
            Reload();

            return true;
        }

        public bool ModifyServerGroup(string groupname, SynapseGroup group)
        {
            if (groupname.Equals("server", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (!_permissionSYML.Sections.Any(x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)))
                return false;

            _ = _permissionSYML.Sections.First(x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)).Value.Import(group);
            _permissionSYML.Store();
            Reload();

            return true;
        }

        public SynapseGroup GetServerGroup(string groupname)
        {
            return !Groups.Keys.Any(x => x.Equals(groupname, StringComparison.OrdinalIgnoreCase))
                ? null
                : groups.FirstOrDefault(x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)).Value.Copy();
        }

        public SynapseGroup GetPlayerGroup(Player player)
        {
            var group = groups.Values.FirstOrDefault(x => x.Members != null && x.Members.Contains(player.UserId));

            if (group != null)
                return group.Copy();

            var nwgroup = GetNorthwoodGroup();

            return player.ServerRoles.Staff && nwgroup != null ? nwgroup : GetDefaultGroup();
        }

        public SynapseGroup GetPlayerGroup(string UserID)
        {
            var group = groups.Values.FirstOrDefault(x => x.Members?.Contains(UserID) ?? false);

            if (group != null)
                return group.Copy();

            var nwgroup = GetNorthwoodGroup();

            return nwgroup != null && UserID.ToLower().Contains("@northwood") ? nwgroup : GetDefaultGroup();
        }

        public SynapseGroup GetDefaultGroup()
        {
            var group = groups.Values.FirstOrDefault(x => x.Default);

            return group != null
                ? group.Copy()
                : new SynapseGroup
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

            _ = RemovePlayerGroup(userid);

            if (group.Members is null)
                group.Members = new List<string>();

            group.Members.Add(userid);

            _ = _permissionSYML.Sections.FirstOrDefault(x => x.Key.Equals(groupname, StringComparison.OrdinalIgnoreCase)).Value.Import(group);
            _permissionSYML.Store();

            Reload();

            return true;
        }

        public bool RemovePlayerGroup(string userid)
        {
            if (!userid.Contains("@"))
                return false;

            var safe = false;
            foreach (var group in groups.Where(x => x.Value.Members?.Contains(userid) ?? false))
            {
                _ = group.Value.Members.Remove(userid);
                _ = _permissionSYML.Sections[group.Key].Import(group.Value);
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
