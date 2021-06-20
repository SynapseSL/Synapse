using System;
using System.Collections.Generic;
using Swan.Formatters;

namespace Synapse.Permission
{
    public class GlobalSynapseGroup
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("color")]
        public string Color { get; set; } = "";

        [JsonProperty("hidden")]
        public bool Hidden { get; set; } = false;

        [JsonProperty("remoteAdmin")]
        public bool RemoteAdmin { get; set; } = false;

        [JsonProperty("permissions")]
        public List<string> Permissions { get; set; } = new List<string>() { };

        [JsonProperty("kickable")]
        public bool Kickable { get; set; } = true;

        [JsonProperty("bannable")]
        public bool Bannable { get; set; } = true;

        [JsonProperty("kick")]
        public bool Kick { get; set; } = false;

        [JsonProperty("ban")]
        public bool Ban { get; set; } = false;

        [JsonProperty("staff")]
        public bool Staff { get; set; } = false;

        public bool HasPermission(string permission)
        {
            if (permission != null && Permissions != null)
                foreach (var perm in Permissions)
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

        public bool HasVanillaPermission(PlayerPermissions permission) => HasPermission(SynapseGroup.VanillaPrefix + "." + permission.ToString());

        public ulong GetVanillaPermissionValue()
        {
            if (Permissions == null) return 0;

            var value = 0ul;
            foreach (var perm in (PlayerPermissions[])Enum.GetValues(typeof(PlayerPermissions)))
            {
                if (HasPermission($"{SynapseGroup.VanillaPrefix}.{perm}"))
                    value += (ulong)perm;
            }

            return value;
        }
    }
}
