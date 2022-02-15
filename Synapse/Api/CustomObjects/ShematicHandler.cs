using Synapse.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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

        public SynapseObject SpawnShematic(string name) => SpawnShematic(GetShematic(name));

        public SynapseObject SpawnShematic(int id) => SpawnShematic(GetShematic(id));

        public SynapseObject SpawnShematic(SynapseShematic shematic) => new SynapseObject(shematic);

        public bool IsIDRegistered(int id) => Shematics.Any(x => x.ID == id);

        public void AddShematic(SynapseShematic shematic, bool removeOnReload = true)
        {
            shematic.reload = removeOnReload;
            var list = Shematics.ToList();
            list.Add(shematic);
            Shematics = list.AsReadOnly();
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
