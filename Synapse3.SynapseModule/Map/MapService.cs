using System.Collections.Generic;
using System.Collections.ObjectModel;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Map;

public class MapService : Service
{
    private readonly RoundEvents _round;

    public MapService(RoundEvents round)
    {
        _round = round;
        round.RoundWaiting.Subscribe(LoadObjects);
        round.RoundRestart.Subscribe(ClearObjects);
    }
    
    //Schematic Objects
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
    internal readonly List<SynapseOldGrenade> _synapseOldGrenades = new();

    //Other Objects
    private readonly List<SynapseTesla> _synapseTeslas = new();
    internal readonly List<SynapseCamera> _synapseCameras = new();
    private readonly List<SynapseElevator> _synapseElevators = new();

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
    public ReadOnlyCollection<SynapseOldGrenade> SynapseOldGrenades => _synapseOldGrenades.AsReadOnly();

    public ReadOnlyCollection<SynapseTesla> SynapseTeslas => _synapseTeslas.AsReadOnly();
    public ReadOnlyCollection<SynapseCamera> SynapseCameras => _synapseCameras.AsReadOnly();
    public ReadOnlyCollection<SynapseElevator> SynapseElevators => _synapseElevators.AsReadOnly();


    private void LoadObjects(RoundWaitingEvent ev)
    {
        foreach (var doorVariant in Synapse.GetObjectsOf<DoorVariant>())
        {
            _ = new SynapseDoor(doorVariant);
        }

        foreach (var generator in Recontainer079.AllGenerators)
        {
            _ = new SynapseGenerator(generator);
        }

        foreach (var locker in Synapse.GetObjectsOf<Locker>())
        {
            _ = new SynapseLocker(locker);
        }

        foreach (var workstation in WorkstationController.AllWorkstations)
        {
            _ = new SynapseWorkStation(workstation);
        }

        foreach (var tesla in Synapse.GetObjectsOf<TeslaGate>())
        {
            _synapseTeslas.Add(new SynapseTesla(tesla));
        }

        foreach (var lift in Synapse.GetObjectsOf<Lift>())
        {
            _synapseElevators.Add(new SynapseElevator(lift));
        }
    }

    private void ClearObjects(RoundRestartEvent ev)
    {
        _synapseTeslas.Clear();
        _synapseElevators.Clear();
        _synapseCameras.Clear();
    }
}