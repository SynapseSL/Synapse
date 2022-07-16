using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Map;

public class MapService : Service
{
    private readonly RoundEvents _round;

    public MapService(RoundEvents round)
    {
        _round = round;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(LoadObjects);
        _round.Restart.Subscribe(ClearObjects);
    }

    public override void Disable()
    {
        _round.Waiting.Subscribe(LoadObjects);
        _round.Restart.Subscribe(ClearObjects);
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
    internal readonly List<SynapseTesla> _synapseTeslas = new();
    internal readonly List<SynapseCamera> _synapseCameras = new();
    internal readonly List<SynapseElevator> _synapseElevators = new();

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



    public Vector3 GlobalRespawnPoint
    {
        get => NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
        set => NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint = value;
    }

    public float HumanWalkSpeed
    {
        get => ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier;
        set => ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier = value;
    }

    public float HumanSprintSpeed
    {
        get => ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier;
        set => ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier = value;
    }
    
    public int Seed => MapGeneration.SeedSynchronizer.Seed;

    public void Explode(Vector3 position, GrenadeType type)
    {
        var item = new SynapseItem((int)type, position);
        item.Throwable.Fuse();
        Timing.CallDelayed(Timing.WaitForOneFrame, item.Destroy);
    }

    public GameObject SpawnTantrum(Vector3 position, float destroy = -1)
    {
        var prefab = NetworkClient.prefabs[Guid.Parse("a0e7ee93-b802-e5a4-38bd-95e27cc133ea")];
        var gameObject = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
        NetworkServer.Spawn(gameObject.gameObject);

        if (destroy >= 0)
            Timing.CallDelayed(destroy,() => NetworkServer.Destroy(gameObject));

        return gameObject;
    }
    
    public void PlaceBlood(Vector3 pos, int type = 0, float size = 2)
        => Synapse.Get<PlayerService>().Host.ClassManager.RpcPlaceBlood(pos, type, size);

    private void LoadObjects(RoundWaitingEvent ev)
    {
        foreach (var doorVariant in Synapse.GetObjects<DoorVariant>())
        {
            _ = new SynapseDoor(doorVariant);
        }

        foreach (var generator in Recontainer079.AllGenerators)
        {
            _ = new SynapseGenerator(generator);
        }

        foreach (var locker in Synapse.GetObjects<Locker>())
        {
            _ = new SynapseLocker(locker);
        }

        foreach (var workstation in WorkstationController.AllWorkstations)
        {
            _ = new SynapseWorkStation(workstation);
        }

        foreach (var tesla in Synapse.GetObjects<TeslaGate>())
        {
            _synapseTeslas.Add(new SynapseTesla(tesla));
        }

        foreach (var lift in Synapse.GetObjects<Lift>())
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