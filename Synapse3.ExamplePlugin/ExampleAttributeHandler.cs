using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Map.Schematic.CustomAttributes;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

[Automatic]
public class ExampleAttributeHandler : AttributeHandler
{
    public override string Name => "ExampleHandler";

    public override void Init()
    {
        NeuronLogger.For<ExamplePlugin>().Warn("Loaded ExampleHandler!");
    }

    public override void OnUpdate(ISynapseObject synapseObject)
    {
        synapseObject.Position += Vector3.up / 120;
    }
}