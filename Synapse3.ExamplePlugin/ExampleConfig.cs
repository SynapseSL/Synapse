using System.Collections.Generic;
using Neuron.Core.Meta;
using Syml;
using Synapse3.SynapseModule.Role;

namespace Synapse3.ExamplePlugin;

[Automatic]
[DocumentSection("Example")]
public class ExampleConfig : IDocumentSection
{
    public int ConfigValue { get; set; } = 5;
    
    public List<uint> ItemsWithMessage { get; set; } = new()
    {
        (uint)ItemType.Medkit,
        (uint)ItemType.Adrenaline
    };

    public ExampleAbstractRole.Config AbstractRoleConfig { get; set; } = new ExampleAbstractRole.Config();
}