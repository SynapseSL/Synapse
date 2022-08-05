using System;
using System.Collections.Generic;
using System.ComponentModel;
using Syml;
using YamlDotNet.Serialization;

namespace Synapse3.SynapseModule.Permissions;

public class SynapseGroup : IDocumentSection
{
    [YamlIgnore]
    public uint GroupId { get; internal set; }
    
    [Description("If Enabled this Group will be assigned to all players, which are in no other Group")]
    public bool Default;

    [Description("If Enabled this Group will be assigned to Northwood staff players, which are in no other Group")]
    public bool Northwood;

    [Description("If Enabled this Group has Acces to RemoteAdmin")]
    public bool RemoteAdmin;

    [Description("The Badge which will be displayed in game")]
    public string Badge = "NONE";

    [Description("The Color which the Badge has in game")]
    public string Color = "NONE";

    [Description("If Enabled The Badge of this Group will be displayed instead of the global Badge")]
    public bool Cover;

    [Description("If Enabled the Badge is Hidden by default")]
    public bool Hidden;

    [Description("The KickPower the group has")]
    public byte KickPower;

    [Description("The KickPower which is required to kick the group")]
    public byte RequiredKickPower = 1;

    [Description("The Permissions which the group has")]
    public List<string> Permissions = new() { };

    [Description("Gives the Group the Permissions of all Groups in this List")]
    public List<string> Inheritance = new() { };

    [Description("The UserID's of the Players in the Group")]
    public List<string> Members = new() { };

    public bool HasPermission(string permission) => HasPermission(permission, 0);

    public bool HasPermission(string permission, int count)
    {
        if (count > 50) return false;

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

        if (Inheritance == null) return false;

        foreach (var groupname in Inheritance)
        {
            if (groupname == null) continue;
            var group = Synapse.Get<PermissionService>().Groups[groupname];
            if (group == null)
                continue;

            if (group.HasPermission(permission, count + 1)) return true;
        }

        return false;
    }

    public bool HasVanillaPermission(PlayerPermissions permission) =>
        HasPermission(VanillaPrefix + "." + permission.ToString());

    public ulong GetVanillaPermissionValue()
    {
        if (Permissions == null) return 0;

        var value = 0ul;
        foreach (var perm in (PlayerPermissions[]) Enum.GetValues(typeof(PlayerPermissions)))
        {
            if (HasPermission($"{VanillaPrefix}.{perm}"))
                value += (ulong) perm;
        }

        return value;
    }

    public SynapseGroup Copy() => new()
    {
        GroupId = GroupId,
        Badge = Badge,
        Color = Color,
        Cover = Cover,
        Default = Default,
        Hidden = Hidden,
        Inheritance = Inheritance,
        KickPower = KickPower,
        Members = Members,
        Northwood = Northwood,
        Permissions = Permissions,
        RemoteAdmin = RemoteAdmin,
        RequiredKickPower = RequiredKickPower
    };

    private const string VanillaPrefix = "vanilla";
}