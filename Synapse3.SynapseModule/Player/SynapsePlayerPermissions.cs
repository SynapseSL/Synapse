using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Permissions;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    /// <summary>
    /// True if the Rank is not visible for normal players
    /// </summary>
    public bool HideRank
    {
        get => !string.IsNullOrEmpty(ServerRoles.HiddenBadge);
        set
        {
            if (value)
                ClassManager.CmdRequestHideTag();
            else
                ClassManager.UserCode_CmdRequestShowTag(false);
        }
    }

    private SynapseGroup _synapseGroup;
    /// <summary>
    /// The Current SynapseRole and therefore all Permissions of the Player
    /// </summary>
    public SynapseGroup SynapseGroup
    {
        get
        {
            if (_synapseGroup == null)
                return Synapse.Get<PermissionService>().GetPlayerGroup(this);

            return _synapseGroup;
        }
        set
        {
            if (value == null)
                return;

            _synapseGroup = value;

            RefreshPermission(HideRank);
        }
    }
    
    /// <summary>
    /// True if the Player can Open the RemoteAdmin
    /// </summary>
    public bool RemoteAdminAccess
    {
        get => ServerRoles.RemoteAdmin;
        set
        {
            if (value)
                RaLogin();
            else
                RaLogout();
        }
    }
    
    /// <summary>
    /// Gives the Player access to the RemoteAdmin (doesn't automatically give any Permissions)
    /// </summary>
    public void RaLogin()
    {
        ServerRoles.RemoteAdmin = true;
        ServerRoles.Permissions = SynapseGroup.GetVanillaPermissionValue() | ServerRoles._globalPerms;
        ServerRoles.RemoteAdminMode = GlobalRemoteAdmin ? ServerRoles.AccessMode.GlobalAccess : ServerRoles.AccessMode.PasswordOverride;
        if (!ServerRoles.AdminChatPerms)
            ServerRoles.AdminChatPerms = SynapseGroup.HasVanillaPermission(PlayerPermissions.AdminChat);
        ServerRoles.TargetOpenRemoteAdmin(false);
    }

    /// <summary>
    /// Removes the Player access to the RemoteAdmin
    /// </summary>
    public void RaLogout()
    {
        Hub.serverRoles.RemoteAdmin = false;
        Hub.serverRoles.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
        Hub.serverRoles.TargetCloseRemoteAdmin();
    }

    /// <summary>
    /// Returns true if the Player has the Permission
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    public bool HasPermission(string permission) =>
        PlayerType == PlayerType.Server || SynapseGroup.HasPermission(permission);

    /// <summary>
    /// Reloads the Permissions of the Player
    /// </summary>
    /// <param name="disp"></param>
    public void RefreshPermission(bool disp)
    {
        var group = new UserGroup
        {
            BadgeText = SynapseGroup.Badge.ToUpper() == "NONE" ? null : SynapseGroup.Badge,
            BadgeColor = SynapseGroup.Color.ToUpper() == "NONE" ? null : SynapseGroup.Color,
            Cover = SynapseGroup.Cover,
            HiddenByDefault = SynapseGroup.Hidden,
            KickPower = SynapseGroup.KickPower,
            Permissions = SynapseGroup.GetVanillaPermissionValue(),
            RequiredKickPower = SynapseGroup.RequiredKickPower,
            Shared = false
        };

        var globalAccesAllowed = true;
        /*
        switch (ServerRoles.GlobalBadgeType)
        {
            case 1:
                globalAccesAllowed = Server.Get.PermissionHandler.serverSection.StaffAccess;
                break;
            case 2:
                globalAccesAllowed = Server.Get.PermissionHandler.serverSection.ManagerAccess;
                break;
            case 3:
                globalAccesAllowed = Server.Get.PermissionHandler.serverSection.GlobalBanTeamAccess;
                break;
            case 4:
                globalAccesAllowed = Server.Get.PermissionHandler.serverSection.GlobalBanTeamAccess;
                break;
        }
        TODO: Implement
        */

        if (GlobalPerms != 0 && globalAccesAllowed)
            group.Permissions |= GlobalPerms;

        ServerRoles.Group = group;
        ServerRoles.Permissions = group.Permissions;
        RemoteAdminAccess = SynapseGroup.RemoteAdmin || GlobalRemoteAdmin;
        ServerRoles.AdminChatPerms = PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.AdminChat);
        ServerRoles._badgeCover = group.Cover;
        QueryProcessor.GameplayData = PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.GameplayData);

        //Since OverwatchPermitted is a seperate vanilla Central Server Permission it is only activated and never deactivated
        if (!ServerRoles.OverwatchPermitted &&
            PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.Overwatch))
            ServerRoles.OverwatchPermitted = true;

        ServerRoles.SendRealIds();

        if (string.IsNullOrEmpty(group.BadgeText))
        {
            ServerRoles.SetColor(null);
            ServerRoles.SetText(null);
            if (!string.IsNullOrEmpty(ServerRoles.PrevBadge))
            {
                ServerRoles.HiddenBadge = ServerRoles.PrevBadge;
                ServerRoles.GlobalHidden = true;
                ServerRoles.RefreshHiddenTag();
            }
        }
        else
        {
            if (ServerRoles._hideLocalBadge || (group.HiddenByDefault && !disp && !ServerRoles._neverHideLocalBadge))
            {
                ServerRoles.Network_myText = null;
                ServerRoles.Network_myColor = "default";
                ServerRoles.HiddenBadge = group.BadgeText;
                ServerRoles.RefreshHiddenTag();
                ServerRoles.TargetSetHiddenRole(Connection, group.BadgeText);
            }
            else
            {
                ServerRoles.HiddenBadge = null;
                ServerRoles.RpcResetFixed();
                ServerRoles.Network_myText = group.BadgeText;
                ServerRoles.Network_myColor = group.BadgeColor;
            }
        }

        var flag = ServerRoles.Staff ||
                   PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.ViewHiddenBadges);
        var flag2 = ServerRoles.Staff ||
                    PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.ViewHiddenGlobalBadges);

        if (flag || flag2)
            foreach (var player in Synapse.Get<PlayerService>().Players)
            {
                if (!string.IsNullOrEmpty(player.ServerRoles.HiddenBadge) &&
                    (!player.ServerRoles.GlobalHidden || flag2) && (player.ServerRoles.GlobalHidden || flag))
                    player.ServerRoles.TargetSetHiddenRole(Connection, player.ServerRoles.HiddenBadge);
            }
    }
    
    /// <summary>
    /// If the Player has Globally Permissions for RemoteAdmin
    /// </summary>
    public bool GlobalRemoteAdmin => ServerRoles.RemoteAdminMode == ServerRoles.AccessMode.GlobalAccess;
    
    /// <summary>
    /// The Global Permissions of the Player
    /// </summary>
    public ulong GlobalPerms => ServerRoles._globalPerms;
}