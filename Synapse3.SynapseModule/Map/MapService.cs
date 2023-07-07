using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hazards;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles.Ragdolls;
using PluginAPI.Core;
using RelativePositioning;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map.Schematic;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Utils.NonAllocLINQ;
using static UnityEngine.PlayerLoop.PreLateUpdate;
using Object = UnityEngine.Object;

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
        RagdollManager.OnRagdollSpawned += RagDollSpawned;
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(LoadObjects);
        _round.Restart.Unsubscribe(ClearObjects);
        RagdollManager.OnRagdollSpawned -= RagDollSpawned;
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
    internal readonly List<SynapseRagDoll> _synapseRagdolls = new();
    internal readonly List<SynapseSchematic> _synapseSchematics = new();

    //Other Objects
    internal readonly List<SynapseTesla> _synapseTeslas = new();
    internal readonly List<SynapseCamera> _synapseCameras = new();

    public ReadOnlyCollection<ISynapseObject> SynapseObjects => _synapseObjects.AsReadOnly();
    public ReadOnlyCollection<SynapseDoor> SynapseDoors => _synapseDoors.AsReadOnly();
    public ReadOnlyCollection<SynapseGenerator> SynapseGenerators => _synapseGenerators.AsReadOnly();
    public ReadOnlyCollection<SynapseCustomObject> SynapseCustomObjects => _synapseCustomObjects.AsReadOnly();
    public ReadOnlyCollection<SynapseLight> SynapseLights => _synapseLights.AsReadOnly();
    public ReadOnlyCollection<SynapseLocker> SynapseLockers => _synapseLockers.AsReadOnly();
    public ReadOnlyCollection<SynapsePrimitive> SynapsePrimitives => _synapsePrimitives.AsReadOnly();
    public ReadOnlyCollection<SynapseTarget> SynapseTargets => _synapseTargets.AsReadOnly();
    public ReadOnlyCollection<SynapseWorkStation> SynapseWorkStations => _synapseWorkStations.AsReadOnly();
    public ReadOnlyCollection<SynapseRagDoll> SynapseRagDolls => _synapseRagdolls.AsReadOnly();
    public ReadOnlyCollection<SynapseSchematic> SynapseSchematics => _synapseSchematics.AsReadOnly();

    public ReadOnlyCollection<SynapseTesla> SynapseTeslas => _synapseTeslas.AsReadOnly();
    public ReadOnlyCollection<SynapseCamera> SynapseCameras => _synapseCameras.AsReadOnly();



    public Vector3 GlobalRespawnPoint
    {
        get => NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
        set => NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint = value;
    }

    public int Seed => SeedSynchronizer.Seed;

    public void Explode(Vector3 position, GrenadeType type, SynapsePlayer owner = null)
    {
        var item = new SynapseItem((uint)type, position);
        item.Throwable.Fuse(owner);
        item.Throwable.FuseTime = 0.01;
        Timing.CallDelayed(0.1f, item.Destroy);
    }

    public GameObject SpawnTantrum(Vector3 position, float destroy = -1)
    {
        var prefab = NetworkClient.prefabs[1306864341];
        var gameObject = Object.Instantiate(prefab, position, Quaternion.identity);
        var comp = gameObject.GetComponent<TantrumEnvironmentalHazard>();
        comp.SynchronizedPosition = new RelativePosition(position);
        NetworkServer.Spawn(gameObject.gameObject);

        if (destroy >= 0)
            Timing.CallDelayed(destroy,() => NetworkServer.Destroy(gameObject));

        return gameObject;
    }


    private void LoadObjects(RoundWaitingEvent ev)
    {
        foreach (var doorVariant in Synapse.GetObjects<DoorVariant>())
        {
            _ = new SynapseDoor(doorVariant);
        }
        
        foreach (var generator in Synapse.GetObjects<Scp079Generator>())
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
    }

    private void ClearObjects(RoundRestartEvent ev)
    {
        _synapseTeslas.Clear();
        _synapseCameras.Clear();
    }

    private void RagDollSpawned(BasicRagdoll rag)
    {
        if(_synapseRagdolls.Any(x => x.BasicRagDoll == rag)) return;
        _ = new SynapseRagDoll(rag);
    }

    public DoorType GetDoorByName(string doorName)
    {
        if (_doorByName.ContainsKey(doorName))
            return _doorByName[doorName];
        
        var newKey = doorName.Split('(')[0];
        return _doorByName.ContainsKey(newKey) ? _doorByName[newKey] : DoorType.Other;
    }


    private readonly Dictionary<string, DoorType> _doorByName = new()
    {
        { "LCZ BreakableDoor", DoorType.LczDoor },
        { "LCZ BreakableDoor ", DoorType.LczDoor },
        { "HCZ BreakableDoor", DoorType.HczDoor },
        { "HCZ BreakableDoor ", DoorType.HczDoor },
        { "EZ BreakableDoor", DoorType.EzDoor },
        { "EZ BreakableDoor ", DoorType.EzDoor },
        { "LCZ PortallessBreakableDoor", DoorType.Airlock },
        { "LCZ PortallessBreakableDoor ", DoorType.Airlock },
        { "Prison BreakableDoor", DoorType.PrisonDoor },
        { "Prison BreakableDoor ", DoorType.PrisonDoor },
        
        { "LCZ_CAFE", DoorType.Pc },
        { "LCZ_WC", DoorType.Wc },
        { "LCZ_ARMORY", DoorType.LczArmory },
        
        { "GR18", DoorType.Gr18 },
        { "GR18_INNER", DoorType.Gr18Inner },
        
        { "330", DoorType.Scp330 },
        { "330_CHAMBER", DoorType.Scp330Chamber },
        
        { "173_BOTTOM", DoorType.Scp173Bottom },
        { "173_ARMORY", DoorType.Scp173Armory },
        { "173_CONNECTOR", DoorType.Scp173Connector },
        { "173_GATE", DoorType.Scp173Gate },
        
        { "914", DoorType.Scp914 },
        { "914 Door", DoorType.Scp914Door },
        { "914 Door ", DoorType.Scp914Door },
        
        { "CHECKPOINT_LCZ_A", DoorType.CheckpointLczA },
        { "CHECKPOINT_LCZ_B", DoorType.CheckpointLczB },
        { "CHECKPOINT_EZ_HCZ", DoorType.Checkpoint },
        
        { "HID_LEFT", DoorType.MicroLeft },
        { "HID", DoorType.Micro },
        { "HID_RIGHT", DoorType.MicroRight },
        { "NUKE_ARMORY", DoorType.NukeArmory },
        { "HCZ_ARMORY", DoorType.HczArmory },
        
        { "SERVERS_BOTTOM", DoorType.ServersBottom },
        
        { "106_PRIMARY", DoorType.Scp106Primary },
        { "106_SECONDARY", DoorType.Scp106Secondary },

        { "Unsecured Pryable GateDoor", DoorType.Scp049Gate },
        { "049_ARMORY", DoorType.Scp049Armory },
        
        { "079_FIRST", DoorType.Scp079First },
        { "079_SECOND", DoorType.Scp079Second },
        
        { "096", DoorType.Scp096 },
        
        { "INTERCOM", DoorType.Intercom },
        { "GATE_A", DoorType.GateA },
        { "GATE_B", DoorType.GateB },
        
        { "SURFACE_NUKE", DoorType.SurfaceNuke },
        { "SURFACE_GATE", DoorType.SurfaceGate },
        { "ESCAPE_PRIMARY", DoorType.EscapePrimary },
        { "ESCAPE_SECONDARY", DoorType.EscapeSecondary },
        { "", DoorType.Other }
    };
}