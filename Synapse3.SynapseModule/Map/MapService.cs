using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Map;

public class MapService : Service
{
    internal readonly List<ISynapseObject> _synapseObjects = new();
    internal readonly List<SynapseDoor> _synapseDoors = new();
    internal readonly List<SynapseGenerator> _synapseGenerators = new();
    internal readonly List<SynapseCustomObject> _synapseCustomObjects = new();
    internal readonly List<SynapseLight> _synapseLights = new();
    internal readonly List<SynapseLocker> _synapseLockers = new();
    internal readonly List<SynapsePrimitive> _synapsePrimitives = new();
    internal readonly List<SynapseTarget> _synapseTargets = new();
    internal readonly List<SynapseWorkStation> _synapseWorkStations = new();
    internal readonly List<SynapseRagdoll> _synapseRagdolls = new();
    internal readonly List<SynapseSchematic> _synapseSchematics = new();
    
    public ReadOnlyCollection<ISynapseObject> SynapseObjects => _synapseObjects.AsReadOnly();
    public ReadOnlyCollection<SynapseDoor> SynapseDoors => _synapseDoors.AsReadOnly();
    public ReadOnlyCollection<SynapseGenerator> SynapseGenerators => _synapseGenerators.AsReadOnly();
    public ReadOnlyCollection<SynapseCustomObject> SynapseCustomObjects => _synapseCustomObjects.AsReadOnly();
    public ReadOnlyCollection<SynapseLight> SynapseLights => _synapseLights.AsReadOnly();
    public ReadOnlyCollection<SynapseLocker> SynapseLockers => _synapseLockers.AsReadOnly();
    public ReadOnlyCollection<SynapsePrimitive> SynapsePrimitives => _synapsePrimitives.AsReadOnly();
    public ReadOnlyCollection<SynapseTarget> SynapseTargets => _synapseTargets.AsReadOnly();
    public ReadOnlyCollection<SynapseWorkStation> SynapseWorkStations => _synapseWorkStations.AsReadOnly();
    public ReadOnlyCollection<SynapseRagdoll> SynapseRagdolls => _synapseRagdolls.AsReadOnly();
    public ReadOnlyCollection<SynapseSchematic> SynapseSchematics => _synapseSchematics.AsReadOnly();
}