using System;
using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Configuration Section for all Join Messages
/// </summary>
[Serializable]
[DocumentSection("JoinMessage")]
public class JoinMessageConfiguration : IDocumentSection
{
    [Description("Displays a Broadcast when a Player joins the Server. Leave empty for none")]
    public string Broadcast { get; set; } = string.Empty;
    
    [Description("How long the Broadcast will be displayed")]
    public ushort BroadcastDuration { get; set; } = 5;

    [Description("Displays a Hint when a Player joins the Server. Leave empty for none")]
    public string Hint { get; set; } = string.Empty;
    [Description("How long the Hint will be displayed")]
    public float HintDuration { get; set; } = 5;

    [Description("Opens a Report Window and displays the Message when a Player joins the Server. Leave empty for none")]
    public string Window { get; set; } = string.Empty;
}