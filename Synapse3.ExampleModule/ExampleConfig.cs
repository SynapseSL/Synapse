using System.Collections.Generic;
using Neuron.Core.Meta;
using Syml;

namespace Synapse3.ExampleModule;

[Automatic]
[DocumentSection("Example")]
public class ExampleConfig : IDocumentSection
{
    public int ConfigValue { get; set; } = 5;
}