using AdminToys;
using Mirror;
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
        internal SchematicHandler() { }

        public static SchematicHandler Get => Server.Get.Schematic;

        internal void Init()
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
                }
            }

            Load();
        }

        public ReadOnlyCollection<SynapseSchematic> Schematics { get; private set; } = new List<SynapseSchematic>().AsReadOnly();

        public SynapseSchematic GetSchematic(int id) => Schematics.FirstOrDefault(x => x.ID == id);

        public SynapseSchematic GetSchematic(string name) => Schematics.FirstOrDefault(x => x.Name == name);

        public SynapseObject SpawnSchematic(string name, Vector3 position) => SpawnSchematic(GetSchematic(name), position);

        public SynapseObject SpawnSchematic(int id, Vector3 position) => SpawnSchematic(GetSchematic(id), position);

        public SynapseObject SpawnSchematic(SynapseSchematic schematic, Vector3 position)
        {
            if (schematic == null) return null;

            var so = new SynapseObject(schematic);
            so.Position = position;
            return so;
        }

        public bool IsIDRegistered(int id) => Schematics.Any(x => x.ID == id);

        public void AddSchematic(SynapseSchematic schematic, bool removeOnReload = true)
        {
            if (IsIDRegistered(schematic.ID)) return;
            schematic.reload = removeOnReload;
            var list = Schematics.ToList();
            list.Add(schematic);
            Schematics = list.AsReadOnly();
        }

        public void SaveSchematic(SynapseSchematic schematic, string fileName)
        {
            if (IsIDRegistered(schematic.ID)) return;
            AddSchematic(schematic);

            var syml = new SYML(Path.Combine(Server.Get.Files.SchematicDirectory, fileName + ".syml"));
            var section = new ConfigSection { Section = schematic.Name };
            section.Import(schematic);
            syml.Sections.Add(schematic.Name, section);
            syml.Store();
        }

        internal void Load()
        {
            var list = Schematics.ToList();

            foreach(var schematic in list.ToList())
                if(schematic.reload) list.Remove(schematic);

            Schematics = list.AsReadOnly();

            foreach(var file in Directory.GetFiles(Server.Get.Files.SchematicDirectory, "*.syml"))
            {
                try
                {
                    var syml = new SYML(file);
                    syml.Load();
                    if (syml.Sections.Count == 0) continue;
                    var section = syml.Sections.First().Value;
                    var schematic = section.LoadAs<SynapseSchematic>();

                    if (IsIDRegistered(schematic.ID)) continue;

                    AddSchematic(schematic);
                }
                catch(Exception ex)
                {
                    Logger.Get.Error($"Synapse-Schematic: Loading Schematic failed - path: {file}\n{ex}");
                }
            }
        }
    }
}
