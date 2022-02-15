using Synapse.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class ShematicHandler
    {
        internal ShematicHandler() { }

        internal void Init()
        {
            PrimitiveSynapseObject.Init();
            Load();
        }

        public ReadOnlyCollection<SynapseShematic> Shematics { get; private set; } = new List<SynapseShematic>().AsReadOnly();

        public SynapseShematic GetShematic(int id) => Shematics.FirstOrDefault(x => x.ID == id);

        public SynapseShematic GetShematic(string name) => Shematics.FirstOrDefault(x => x.Name == name);

        public SynapseObject SpawnShematic(string name, Vector3 position) => SpawnShematic(GetShematic(name), position);

        public SynapseObject SpawnShematic(int id, Vector3 position) => SpawnShematic(GetShematic(id), position);

        public SynapseObject SpawnShematic(SynapseShematic shematic, Vector3 position)
        {
            var so = new SynapseObject(shematic);
            so.Position = position;
            return so;
        }

        public bool IsIDRegistered(int id) => Shematics.Any(x => x.ID == id);

        public void AddShematic(SynapseShematic shematic, bool removeOnReload = true)
        {
            if (IsIDRegistered(shematic.ID)) return;
            shematic.reload = removeOnReload;
            var list = Shematics.ToList();
            list.Add(shematic);
            Shematics = list.AsReadOnly();
        }

        public void SaveShematic(SynapseShematic shematic, string fileName)
        {
            if (IsIDRegistered(shematic.ID)) return;
            AddShematic(shematic);

            var syml = new SYML(Path.Combine(Server.Get.Files.ShematicDirectory, fileName + ".syml"));
            var section = new ConfigSection { Section = shematic.Name };
            section.Import(shematic);
            syml.Sections.Add(shematic.Name, section);
            syml.Store();
        }

        internal void Load()
        {
            var list = Shematics.ToList();

            foreach(var shematic in list.ToList())
                if(shematic.reload) list.Remove(shematic);

            Shematics = list.AsReadOnly();

            foreach(var file in Directory.GetFiles(Server.Get.Files.ShematicDirectory, "*.syml"))
            {
                try
                {
                    var syml = new SYML(file);
                    syml.Load();
                    if (syml.Sections.Count == 0) continue;
                    var section = syml.Sections.First().Value;
                    var shematic = section.LoadAs<SynapseShematic>();

                    if (IsIDRegistered(shematic.ID)) continue;

                    AddShematic(shematic);
                }
                catch(Exception ex)
                {
                    Logger.Get.Error($"Synapse-Shematic: Loading Shematic failed - path: {file}\n{ex}");
                }
            }
        }
    }
}
