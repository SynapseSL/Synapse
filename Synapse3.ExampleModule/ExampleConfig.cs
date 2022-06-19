using System.Collections.Generic;
using Neuron.Core.Meta;
using Syml;

namespace Synapse3.ExampleModule;

[Automatic]
[DocumentSection("Example")]
public class ExampleConfig : IDocumentSection
{
    public string StringEntry { get; set; } = "DefaultStringValue";
    public int IntEntry { get; set; } = 1337;
    public List<string> ListEntry { get; set; } = new(new[] {"Entry 1", "Entry 2", "Entry 3"});
}