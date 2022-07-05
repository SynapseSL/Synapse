using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Schematic.CustomAttributes;
using Synapse3.SynapseModule.Player;

namespace Synapse3.ExamplePlugin;

[Automatic]
public class ExampleAttributeHandler : AttributeHandler
{
    public ExampleAttributeHandler(PlayerService service)
    {
        
    }
    
    public override string Name => "ExampleHandler";

    public override void Init()
    {
        NeuronLogger.For<ExamplePlugin>().Warn("Loaded ExampleHandler!");
    }
}