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

        private Dictionary<string, SynapseGroup> Groups = new Dictionary<string, SynapseGroup>();
        private ServerSection ServerSection;

        internal void Init()
        {
            _permissionSYML = new SYML(Server.Get.Files.PermissionFile);
            _permissionSYML.Load();
            ServerSection = new ServerSection();
            ServerSection = _permissionSYML.GetOrSetDefault("Server", ServerSection);

            foreach(var pair in _permissionSYML.Sections)
                if(pair.Key.ToLower() != "server")
                {
                    try
                    {
                        var group = pair.Value.LoadAs<SynapseGroup>();
                        Groups.Add(pair.Key,group);
                    }
                    catch(Exception e)
                    {
                        Logger.Get.Error($"Synapse-Permission: Section {pair.Key} in permission.syml is no SynapseGroup or ServerGroup\n{e}");
                    }
                }

            if(Groups.Count == 0)
            {
                var group = new SynapseGroup()
                {
                    Badge = "Owner",
                    Color = "red",
                    Cover = true,
                    Hidden = true,
                    KickPower = 254,
                    Members = new List<string> { "0000000@steam" },
                    Permissions = new List<string> { "*" },
                    RemoteAdmin = true,
                    RequiredKickPower = 255
                };

                AddServerGroup(group, "Owner");
            }

            //TODO: fix bug and remove debug message
            foreach (var members in Groups.Values.First().Members)
                Logger.Get.Info(members);
        }

        public void AddServerGroup(SynapseGroup group,string groupname)
        {
            group = _permissionSYML.GetOrSetDefault(groupname, group);
            Groups.Add(groupname,group);
        }

        public SynapseGroup GetServerGroup(string groupname) => Groups.FirstOrDefault(x => x.Key.ToLower() == groupname.ToLower()).Value;

        public SynapseGroup GetPlayerGroup(string UserID) => Groups.Values.FirstOrDefault(x => x.Members.Contains(UserID));
    }
}
