using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Map;

public class MapService : Service
{
    internal readonly List<ISynapseObject> _synapseObjects = new();
    public ReadOnlyCollection<ISynapseObject> SynapseObjects => _synapseObjects.AsReadOnly();

    internal readonly List<SynapseDoor> _synapseDoors = new();
    public ReadOnlyCollection<SynapseDoor> SynapseDoors => _synapseDoors.AsReadOnly();
    
    internal readonly List<SynapseGenerator> _synapseGenerators = new();
    public ReadOnlyCollection<SynapseGenerator> SynapseGenerators => _synapseGenerators.AsReadOnly();

    internal readonly List<SynapseCustomObject> _synapseCustomObjects = new();
    public ReadOnlyCollection<SynapseCustomObject> SynapseCustomObjects => _synapseCustomObjects.AsReadOnly();

    internal readonly List<SynapseLight> _synapseLights = new();
    public ReadOnlyCollection<SynapseLight> SynapseLights => _synapseLights.AsReadOnly();

    internal readonly List<SynapseLocker> _synapseLockers = new();
    public ReadOnlyCollection<SynapseLocker> SynapseLockers => _synapseLockers.AsReadOnly();

    internal readonly List<SynapsePrimitive> _synapsePrimitives = new();
    public ReadOnlyCollection<SynapsePrimitive> SynapsePrimitives => _synapsePrimitives.AsReadOnly();

    internal readonly List<SynapseTarget> _synapseTargets = new();
    public ReadOnlyCollection<SynapseTarget> SynapseTargets => _synapseTargets.AsReadOnly();

    internal readonly List<SynapseWorkStation> _synapseWorkStations = new();
    public ReadOnlyCollection<SynapseWorkStation> SynapseWorkStations => _synapseWorkStations.AsReadOnly();

    internal readonly List<SynapseRagdoll> _synapseRagdolls = new();
    public ReadOnlyCollection<SynapseRagdoll> SynapseRagdolls => _synapseRagdolls.AsReadOnly();
}