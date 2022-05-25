using AdminToys;
using Interactables.Interobjects;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using Mirror;
using Synapse.Api.CustomObjects.CustomAttributes;
using Synapse.Api.Enum;
using Synapse.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SchematicHandler
    {
        internal SchematicHandler()
        {
            AttributeHandler = new CustomAttributeHandler();
        }

        public static SchematicHandler Get
            => Server.Get.Schematic;

        public CustomAttributeHandler AttributeHandler { get; }

        internal void Init()
        {
            try
            {
                foreach (var prefab in NetworkManager.singleton.spawnPrefabs)
                {
                    switch (prefab.name)
                    {
                        case "PrimitiveObjectToy" when prefab.TryGetComponent<PrimitiveObjectToy>(out var pref):
                            SynapsePrimitiveObject.Prefab = pref;
                            break;

                        case "LightSourceToy" when prefab.TryGetComponent<LightSourceToy>(out var lightpref):
                            SynapseLightObject.Prefab = lightpref;
                            break;

                        case "sportTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                            SynapseTargetObject.Prefabs[TargetType.Sport] = target;
                            break;

                        case "dboyTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                            SynapseTargetObject.Prefabs[TargetType.DBoy] = target;
                            break;

                        case "binaryTargetPrefab" when prefab.TryGetComponent<ShootingTarget>(out var target):
                            SynapseTargetObject.Prefabs[TargetType.Binary] = target;
                            break;

                        case "Work Station" when prefab.TryGetComponent<WorkstationController>(out var station):
                            SynapseWorkStationObject.Prefab = station;
                            break;

                        case "EZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                            SynapseDoorObject.Prefab[SpawnableDoorType.EZ] = door;
                            break;

                        case "HCZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                            SynapseDoorObject.Prefab[SpawnableDoorType.HCZ] = door;
                            break;

                        case "LCZ BreakableDoor" when prefab.TryGetComponent<BreakableDoor>(out var door):
                            SynapseDoorObject.Prefab[SpawnableDoorType.LCZ] = door;
                            break;
                    }
                }

                Load();
                AttributeHandler.Init();
            }
            catch (Exception ex)
            {
                Logger.Get.Error("Synapse-Object: Error while Initialising Synapse Objects and Schematics:\n" + ex);
            }
        }

        internal void InitLate()
        {
            foreach (var prefab in NetworkClient.prefabs)
            {
                switch (prefab.Key.ToString())
                {
                    case "daf3ccde-4392-c0e4-882d-b7002185c6b8" when prefab.Value.TryGetComponent<Scp079Generator>(out var gen):
                        SynapseGeneratorObject.GeneratorPrefab = gen;
                        break;

                    case "68f13209-e652-6024-2b89-0f75fb88a998" when prefab.Value.TryGetComponent<MapGeneration.Distributors.Locker>(out var locker):

                        SynapseLockerObject.Prefabs[LockerType.ScpPedestal] = locker;
                        break;

                    case "5ad5dc6d-7bc5-3154-8b1a-3598b96e0d5b" when prefab.Value.TryGetComponent<MapGeneration.Distributors.Locker>(out var locker):
                        SynapseLockerObject.Prefabs[LockerType.LargeGunLocker] = locker;
                        break;

                    case "850f84ad-e273-1824-8885-11ae5e01e2f4" when prefab.Value.TryGetComponent<MapGeneration.Distributors.Locker>(out var locker):
                        SynapseLockerObject.Prefabs[LockerType.RifleRackLocker] = locker;
                        break;

                    case "d54bead1-286f-3004-facd-74482a872ad8" when prefab.Value.TryGetComponent<MapGeneration.Distributors.Locker>(out var locker):
                        SynapseLockerObject.Prefabs[LockerType.StandardLocker] = locker;
                        break;

                    case "5b227bd2-1ed2-8fc4-2aa1-4856d7cb7472" when prefab.Value.TryGetComponent<MapGeneration.Distributors.Locker>(out var locker):
                        SynapseLockerObject.Prefabs[LockerType.MedkitWallCabinet] = locker;
                        break;

                    case "db602577-8d4f-97b4-890b-8c893bfcd553" when prefab.Value.TryGetComponent<MapGeneration.Distributors.Locker>(out var locker):
                        SynapseLockerObject.Prefabs[LockerType.AdrenalineWallCabinet] = locker;
                        break;
                }
            }

            foreach (var role in CharacterClassManager._staticClasses)
            {
                if (role != null)
                    SynapseRagdollObject.Prefabs[role.roleId] = role.model_ragdoll?.GetComponent<global::Ragdoll>();
            }
        }

        public ReadOnlyCollection<SynapseSchematic> Schematics { get; private set; } = new List<SynapseSchematic>().AsReadOnly();

        public SynapseSchematic GetSchematic(int id) => Schematics.FirstOrDefault(x => x.ID == id);

        public SynapseSchematic GetSchematic(string name) => Schematics.FirstOrDefault(x => x.Name == name);

        public SynapseObject SpawnSchematic(string name, Vector3 position) => SpawnSchematic(GetSchematic(name), position);

        public SynapseObject SpawnSchematic(string name, Vector3 position, Vector3 rotation) => SpawnSchematic(GetSchematic(name), position, Quaternion.Euler(rotation));

        public SynapseObject SpawnSchematic(string name, Vector3 position, Quaternion rotation) => SpawnSchematic(GetSchematic(name), position, rotation);

        public SynapseObject SpawnSchematic(int id, Vector3 position) => SpawnSchematic(GetSchematic(id), position);

        public SynapseObject SpawnSchematic(int id, Vector3 position, Vector3 rotation) => SpawnSchematic(GetSchematic(id), position, Quaternion.Euler(rotation));

        public SynapseObject SpawnSchematic(int id, Vector3 position, Quaternion rotation) => SpawnSchematic(GetSchematic(id), position, rotation);

        public SynapseObject SpawnSchematic(SynapseSchematic schematic, Vector3 position, Vector3 rotation) => SpawnSchematic(schematic, position, Quaternion.Euler(rotation));

        public SynapseObject SpawnSchematic(SynapseSchematic schematic, Vector3 position, Quaternion rotation)
        {
            if (schematic is null)
                return null;

            var so = new SynapseObject(schematic)
            {
                Position = position,
                Rotation = rotation
            };
            return so;
        }

        public SynapseObject SpawnSchematic(SynapseSchematic schematic, Vector3 position)
        {
            if (schematic is null)
                return null;

            var so = new SynapseObject(schematic)
            {
                Position = position
            };
            return so;
        }

        public bool IsIDRegistered(int id) => Schematics.Any(x => x.ID == id);

        public void AddSchematic(SynapseSchematic schematic, bool removeOnReload = true)
        {
            if (IsIDRegistered(schematic.ID))
                return;
            schematic.reload = removeOnReload;
            var list = Schematics.ToList();
            list.Add(schematic);
            Schematics = list.AsReadOnly();
        }

        public void SaveSchematic(SynapseSchematic schematic, string fileName)
        {
            if (IsIDRegistered(schematic.ID))
                return;
            AddSchematic(schematic);

            var syml = new SYML(Path.Combine(Server.Get.Files.SchematicDirectory, fileName + ".syml"));
            var section = new ConfigSection { Section = schematic.Name };
            _ = section.Import(schematic);
            syml.Sections.Add(schematic.Name, section);
            syml.Store();
        }

        internal void Load()
        {
            var list = Schematics.ToList();

            foreach (var schematic in list.ToList())
            {
                if (schematic.reload)
                    _ = list.Remove(schematic);
            }

            Schematics = list.AsReadOnly();

            foreach (var file in Directory.GetFiles(Server.Get.Files.SchematicDirectory, "*.syml"))
            {
                try
                {
                    var syml = new SYML(file);
                    syml.Load();
                    if (syml.Sections.Count == 0)
                        continue;
                    var section = syml.Sections.First().Value;
                    var schematic = section.LoadAs<SynapseSchematic>();

                    if (IsIDRegistered(schematic.ID))
                        continue;

                    AddSchematic(schematic);
                }
                catch (Exception ex)
                {
                    Logger.Get.Error($"Synapse-Schematic: Loading Schematic failed - path: {file}\n{ex}");
                }
            }
        }
    }
}
