using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Configuration Section for all Permission related stuff
/// </summary>
[DocumentSection("Permission")]
public class PermissionConfiguration : IDocumentSection
{
    [Description("If Enabled the GlobalBanTeam gets their global Permissions")]
    public bool GlobalBanTeamAccess = true;

    [Description("If Enabled the Manager gets their global Permissions")]
    public bool ManagerAccess = true;

    [Description("If Enabled the Staff gets their global Permissions")]
    public bool StaffAccess = true;

    [Description("If Enabled the RemoteAdmin Player List will be sorted by the Players SynapseGroup")]
    public bool BetterRemoteAdminList = true;

    [Description("If Enabled all Player in Overwatch will be displayed at the bottom of the Remote Admin List")]
    public bool OverWatchListDown = true;
}