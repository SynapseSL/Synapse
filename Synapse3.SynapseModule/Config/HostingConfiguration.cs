using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

[DocumentSection("Hosting")]
public class HostingConfiguration : IDocumentSection
{
    [Description("If enabled your Server is marked as Synapse Server on the Server list")]
    public bool NameTracking { get; set; } = true;
}