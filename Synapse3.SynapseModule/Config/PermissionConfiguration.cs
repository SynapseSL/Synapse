using System;
using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Configuration Section for all Permission related stuff
/// </summary>
[Serializable]
[DocumentSection("Permission")]
public class PermissionConfiguration : IDocumentSection
{
    [Description("If Enabled the GlobalBanTeam gets their global Permissions")]
    public bool GlobalBanTeamAccess { get; set; } = true;

    [Description("If Enabled the Manager gets their global Permissions")]
    public bool ManagerAccess { get; set; } = true;

    [Description("If Enabled the Staff gets their global Permissions")]
    public bool StaffAccess { get; set; } = true;

    [Description("If Enabled the RemoteAdmin Player List will be sorted by the Players SynapseGroup")]
    public bool BetterRemoteAdminList { get; set; } = true;

    [Description("If Enabled the Categories OverWatch, Invisible, GodMode and NoClip are visible")]
    public bool EnableGameModeCategories { get; set; } = true;

    [Description("If Enabled the Players will be displayed below each Category they belong to")]
    public bool DisplayPlayerMultipleTimes { get; set; } = false;

    [Description("How long Synapse waits until Updating the color when a SynapseGroup uses the color Rainbow")]
    public float RainbowUpdateTime { get; set; } = 1f;
}