using Synapse.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Synapse.Permission
{
    public class SynapseGroup : IConfigSection
    {
        [Description("If Enabled this Group will be assigned to all players,which are in no other Group")]
        public bool Default = false;

        [Description("If Enabled this Group will be assigned to Northwood staff players,which are in no other Group")]
        public bool Northwood = false;

        [Description("If Enabled this Group has Acces to RemoteAdmin")]
        public bool RemoteAdmin = false;

        [Description("The Badge which will be displayed in game")]
        public string Badge = "NONE";

        [Description("The Color which the Badge has in game")]
        public string Color = "NONE";

        [Description("If Enabled The Badge of this Group will be displayed instead of the global Badge")]
        public bool Cover = false;

        [Description("If Enabled the Badge is Hidden by default")]
        public bool Hidden = false;

        [Description("The KickPower the group has")]
        public byte KickPower = 0;

        [Description("The KickPower which is required to kick the group")]
        public byte RequiredKickPower = 1;

        [Description("The Permissions which the group has")]
        public List<string> Permissions = new List<string> { };

        [Description("The UserID´s of the Players in the Group")]
        public List<string> Members = new List<string> { };

        public bool HasPermission(string permission)
        {
            if (permission == null || Permissions == null) return false;


            foreach(var perm in Permissions)
            {
                if (perm == "*" || perm == "*.*" || perm == ".*") return true;

                if (permission.ToUpper() == perm.ToUpper()) return true;

                var args = permission.Split('.');
                var args2 = perm.Split('.');

                if (args.Length == 1 || args2.Length == 1) continue;

                if (args2[0].ToUpper() == args[0].ToUpper())
                    for (int i = 1; i < args.Length; i++)
                    {
                        if (args.Length < i + 1 || args2.Length < i + 1) break;

                        if (args2[i] == "*") return true;

                        if (args[i].ToUpper() != args2[i].ToUpper()) break;
                    }
            }

            return false;
        }

        public bool HasVanillaPermission(PlayerPermissions permission) => HasPermission(VanillaPrefix + "." + permission.ToString());

        public ulong GetVanillaPermissionValue()
        {
            if (Permissions.Any(x => x == "*" || x == ".*" || x == "*.*" || x == $"{VanillaPrefix}.*"))
                return FullVanillaPerms();

            var vanillaperms = Permissions.Where(x => x.Split('.')[0].ToLower() == VanillaPrefix);

            List<PlayerPermissions> perms = new List<PlayerPermissions>();
            foreach(var perm in vanillaperms)
                if (Enum.TryParse<PlayerPermissions>(perm.Split('.')[1], out var permenum))
                    perms.Add(permenum);

            ulong Permission = 0;
            foreach (var perm in perms)
                Permission += (ulong)perm;

            return Permission;
        }

        private ulong FullVanillaPerms()
        {
            ulong fullperm = 0;
            foreach (var perm in (PlayerPermissions[])Enum.GetValues(typeof(PlayerPermissions)))
                fullperm += (ulong)perm;

            return fullperm;
        }

        private const string VanillaPrefix = "vanilla";
    }
}
