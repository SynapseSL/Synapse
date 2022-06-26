using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

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