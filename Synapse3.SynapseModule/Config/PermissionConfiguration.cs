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
}