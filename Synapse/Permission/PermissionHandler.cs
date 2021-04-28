using System;
using Synapse.Config;
using System.Collections.Generic;
using Synapse.Api;
using System.Linq;
 
namespace Synapse.Permission
{
    public class PermissionHandler
    {
        internal PermissionHandler() { }

        private SYML _permissionSYML;

        internal readonly Dictionary<string, SynapseGroup> groups = new Dictionary<string, SynapseGroup>();
        internal ServerSection serverSection;

        public static PermissionHandler Get => Server.Get.PermissionHandler;

        public Dictionary<string, SynapseGroup> Groups { get => new Dictionary<string, SynapseGroup>(groups); }

        internal void Init()
        {
            _permissionSYML = new SYML(Server.Get.Files.PermissionFile);
            Reload();
        }

        public void Reload()
        {
            _permissionSYML.Load();
            serverSection = new ServerSection();
            serverSection = _permissionSYML.GetOrSetDefault("Server", serverSection);
            groups.Clear();

            foreach (var pair in _permissionSYML.Sections)
                if (pair.Key.ToLower() != "server")
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

                AddServerGroup(GetDefaultGroup(),"User");
            }

            foreach (var player in Server.Get.Players)
                player.RefreshPermission(player.HideRank);
        }

        public void AddServerGroup(SynapseGroup group,string groupname)
        {
            group = _permissionSYML.GetOrSetDefault(groupname, group);
            groups.Add(groupname,group);
        }

        public SynapseGroup GetServerGroup(string groupname)
        {
            if (!Groups.Keys.Any(x => x.ToLower() == groupname.ToLower())) return null;
            return groups.FirstOrDefault(x => x.Key.ToLower() == groupname.ToLower()).Value;
        }

        public SynapseGroup GetPlayerGroup(Player player)
        {
            var group = groups.Values.FirstOrDefault(x => x.Members != null && x.Members.Contains(player.UserId));

            if (group != null)
                return group;

            var nwgroup = GetNorthwoodGroup();

            if (player.ServerRoles.Staff && nwgroup != null)
                return nwgroup;

            return GetDefaultGroup();
        }

        public SynapseGroup GetPlayerGroup(string UserID)
        {
            var group = groups.Values.FirstOrDefault(x => x.Members != null && x.Members.Contains(UserID));

            if (group != null)
                return group;

            var nwgroup = GetNorthwoodGroup();

            if (UserID.ToLower().Contains("@northwood") && nwgroup != null)
                return nwgroup;

            return GetDefaultGroup();
        }

        public SynapseGroup GetDefaultGroup()
        {
            var group = groups.Values.FirstOrDefault(x => x.Default);

            if (group != null)
                return group;

            return new SynapseGroup
            {
                Default = true,
                Permissions = new List<string> { "synapse.command.help", "synapse.command.plugins" },
                Members = null,
            };
        }

        public SynapseGroup GetNorthwoodGroup() => groups.Values.FirstOrDefault(x => x.Northwood);

        public bool AddPlayerToGroup(string groupname, string userid)
        {
            var group = GetServerGroup(groupname);

            if (group == null)
            {
                Logger.Get.Warn($"Group {groupname} does not exist!");
                return false;
            }

            if (!userid.Contains("@"))
                return false;

            RemovePlayerGroup(userid);

            if (group.Members == null)
                group.Members = new List<string>();

            group.Members.Add(userid);
            
            _permissionSYML.Sections.FirstOrDefault(x => x.Key.ToLower() == groupname.ToLower()).Value.Import(group);
            _permissionSYML.Store();
            
            Reload();

            return true;
        }

        public bool RemovePlayerGroup(string userid)
        {
            if (!userid.Contains("@"))
                return false;

            var safe = false;
            foreach(var group in groups.Where(x => x.Value.Members != null && x.Value.Members.Contains(userid)))
            {
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
