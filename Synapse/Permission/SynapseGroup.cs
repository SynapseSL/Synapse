using Synapse.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Permission
{
    public class SynapseGroup : IConfigSection
    {
        public string Password = "NONE";

        public bool Default = false;

        public bool Northwood = false;

        public bool RemoteAdmin = false;

        public string Badge = "NONE";

        public string Color = "NONE";

        public bool Cover = false;

        public bool Hidden = false;

        public byte KickPower = 0;

        public byte RequiredKickPower = 1;

        public List<string> Permissions = new List<string>();

        public List<string> Members = new List<string>();

        /*public bool HasPermission(string Permission)
        {

        }*/

        public bool HasVanillaPermission(PlayerPermissions permission) => Permissions.Any(x => x.ToLower() == $"{VanillaPrefix}.{permission}".ToLower());

        public ulong GetVanillaPermissionValue()
        {
            var vanillaperms = Permissions.Where(x => x.Split('.')[0].ToLower() == VanillaPrefix);

            List<PlayerPermissions> perms = new List<PlayerPermissions>();
            foreach(var perm in vanillaperms)
                if (Enum.TryParse<PlayerPermissions>(perm, out var permenum))
                    perms.Add(permenum);

            ulong Permission = 0;
            foreach (var perm in perms)
                Permission += (ulong)perm;

            return Permission;
        }

        private const string VanillaPrefix = "vanilla";
    }
}
