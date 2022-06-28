using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AdminToys;
using Interactables.Interobjects;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using Mirror;
using Neuron.Core;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Modules.Configs;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map;

public class SchematicService : Service
{
    private NeuronBase _base;
    private NeuronLogger _logger;
    private RoundEvents _round;
    private List<SchematicConfiguration> _schematicConfigurations = new();

    public ReadOnlyCollection<SchematicConfiguration> SchematicConfigurations => _schematicConfigurations.AsReadOnly();

    public SchematicService(NeuronBase neuronBase, NeuronLogger neuronLogger, RoundEvents round)
    {
        _base = neuronBase;
        _logger = neuronLogger;
        _round = round;
    }

    public void Reload()
    {
        foreach (var configuration in _schematicConfigurations)
        {
            if (configuration.Reload) _schematicConfigurations.Remove(configuration);
        }

        
        foreach (var file in Directory.GetFiles(_base.PrepareRelativeDirectory("Schematics"),"*.syml"))
        {
            try
            {
                var container = new ConfigContainer(_base, _logger, file.Replace(".syml",""));

                if(container.Document.Sections.Count == 0) continue;


                var configuration = (SchematicConfiguration)container.Document.Sections.First().Value.Export(typeof(SchematicConfiguration));

                RegisterSchematic(configuration);
            }
            catch (Exception ex)
            {
                NeuronLogger.For<Synapse>().Error("Sy3 Schematics: Loading schematic failed:\n" + file + "\n" + ex);
            }
        }
        
    }

    public SynapseSchematic SpawnSchematic(SchematicConfiguration configuration, Vector3 position, Quaternion rotation,
        Vector3 scale)
    {
        if (configuration == null) return null;

        var so = new SynapseSchematic(configuration)
        {
            Position = position,
            Rotation = rotation,
            Scale = scale,
        };
        return so;
    }

    public SynapseSchematic SpawnSchematic(SchematicConfiguration configuration, Vector3 position, Quaternion rotation)
        => SpawnSchematic(configuration, position, rotation, Vector3.one);

    public SynapseSchematic SpawnSchematic(SchematicConfiguration configuration, Vector3 position)
        => SpawnSchematic(configuration, position, Quaternion.identity, Vector3.one);


    public SynapseSchematic SpawnSchematic(int id, Vector3 position, Quaternion rotation,
        Vector3 scale)
        => SpawnSchematic(GetConfiguration(id), position, rotation, scale);
    
    public SynapseSchematic SpawnSchematic(int id, Vector3 position, Quaternion rotation)
        => SpawnSchematic(GetConfiguration(id), position, rotation, Vector3.one);
    
    public SynapseSchematic SpawnSchematic(int id, Vector3 position)
        => SpawnSchematic(GetConfiguration(id), position, Quaternion.identity, Vector3.one);

    public SchematicConfiguration GetConfiguration(int id) => _schematicConfigurations.FirstOrDefault(x => x.ID == id);

    public SchematicConfiguration GetConfiguration(string name) =>
        SchematicConfigurations.FirstOrDefault(x => x.Name == name);

    public void RegisterSchematic(SchematicConfiguration configuration, bool removeOnReload = false)
    {
        if(IsIDRegistered(configuration.ID)) return;

        configuration.Reload = removeOnReload;
        _schematicConfigurations.Add(configuration);
    }

    public bool SaveConfiguration(SchematicConfiguration configuration)
    {
        if (IsIDRegistered(configuration.ID) || string.IsNullOrWhiteSpace(configuration.Name)) return false;
        RegisterSchematic(configuration, true);
        
        var file = Path.Combine(_base.PrepareRelativeDirectory("Schematics"), configuration.Name);

        var container = new ConfigContainer(_base, _logger, file);
        container.Document.Set(configuration.Name, configuration);
        container.Store();
        return true;
    }

    public bool IsIDRegistered(int id) => _schematicConfigurations.Any(x => x.ID == id);


    public override void Enable()
    {
        _round.RoundWaiting.Subscribe(LateInit);
        
        foreach (var prefab in NetworkManager.singleton.spawnPrefabs)
        {
            switch (prefab.name)
            {
                case "EZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                    SynapseDoor.Prefab[SynapseDoor.SpawnableDoorType.Ez] = door;
                    break;

                case "HCZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                    SynapseDoor.Prefab[SynapseDoor.SpawnableDoorType.Hcz] = door;
                    break;

                case "LCZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                    SynapseDoor.Prefab[SynapseDoor.SpawnableDoorType.Lcz] = door;
                    break;
                
                case "PrimitiveObjectToy" when prefab.TryGetComponent<PrimitiveObjectToy>(out var pref):
                    SynapsePrimitive.Prefab = pref;
                    break;

                case "LightSourceToy" when prefab.TryGetComponent<LightSourceToy>(out var lightpref):
                    SynapseLight.Prefab = lightpref;
                    break;

                case "sportTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                    SynapseTarget.Prefabs[SynapseTarget.TargetType.Sport] = target;
                    break;

                case "dboyTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                    SynapseTarget.Prefabs[SynapseTarget.TargetType.DBoy] = target;
                    break;

                case "binaryTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                    SynapseTarget.Prefabs[SynapseTarget.TargetType.Binary] = target;
                    break;
                
                case "Work Station" when prefab.TryGetComponent<WorkstationController>(out var station):
                    SynapseWorkStation.Prefab = station;
                    break;
            }
        }
        
        Reload();
    }

    private void LateInit(RoundWaitingEvent ev)
    {
        if(!ev.FirstTime) return;

        foreach (var prefab in NetworkClient.prefabs)
        {
            switch (prefab.Key.ToString())
            {
                case "daf3ccde-4392-c0e4-882d-b7002185c6b8" when prefab.Value.TryGetComponent<Scp079Generator>(out var gen):
                    SynapseGenerator.GeneratorPrefab = gen;
                    break;
                
                case "68f13209-e652-6024-2b89-0f75fb88a998" when prefab.Value.TryGetComponent<Locker>(out var locker):

                    SynapseLocker.Prefabs[SynapseLocker.LockerType.ScpPedestal] = locker;
                    break;

                case "5ad5dc6d-7bc5-3154-8b1a-3598b96e0d5b" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.LargeGunLocker] = locker;
                    break;

                case "850f84ad-e273-1824-8885-11ae5e01e2f4" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.RifleRackLocker] = locker;
                    break;

                case "d54bead1-286f-3004-facd-74482a872ad8" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.StandardLocker] = locker;
                    break;

                case "5b227bd2-1ed2-8fc4-2aa1-4856d7cb7472" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.MedkitWallCabinet] = locker;
                    break;

                case "db602577-8d4f-97b4-890b-8c893bfcd553" when prefab.Value.TryGetComponent<Locker>(out var locker):
                    SynapseLocker.Prefabs[SynapseLocker.LockerType.AdrenalineWallCabinet] = locker;
                    break;
            }
        }
        
        foreach (var role in CharacterClassManager._staticClasses)
            if (role != null)
                SynapseRagdoll.Prefabs[role.roleId] = role.model_ragdoll?.GetComponent<Ragdoll>();
    }
}