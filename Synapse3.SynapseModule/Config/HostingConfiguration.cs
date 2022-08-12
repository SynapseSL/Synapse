using System;
using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Configuration Section for all Hosting related configs
/// </summary>
[Serializable]
[DocumentSection("Hosting")]
public class HostingConfiguration : IDocumentSection
{
    [Description("If enabled your Server is marked as Synapse Server on the Server list")]
    public bool NameTracking { get; set; } = true;

    [Description("A List of Languages that should be used as Server Language. Recommended: Your language first and then English")]
    public string[] Language { get; set; } = new[] { "ENGLISH" };
}