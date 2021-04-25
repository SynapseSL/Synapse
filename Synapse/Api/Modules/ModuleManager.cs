using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Modules
{
    public class ModuleManager
    {
        internal ModuleManager() { }

        public static ModuleManager Get => Server.Get.ModuleManager;

        public List<ISynapseModule> Modules { get; } = new List<ISynapseModule>();

        public TModule GetModule<TModule>() where TModule : ISynapseModule => (TModule)Modules.FirstOrDefault(x => typeof(TModule).IsAssignableFrom(x.GetType()));

        public void Reload()
        {
            foreach (var module in Modules)
                module.Reload();
        }

        internal void Load()
        {
            if (!Directory.Exists(Server.Get.Files.ModuleDirectory)) return;

            foreach(var file in Directory.GetFiles(Server.Get.Files.ModuleDirectory))
                try
                {
                    var assembly = Assembly.Load(File.ReadAllBytes(file));

                    foreach (var type in assembly.GetTypes())
                    {
                        if (!typeof(ISynapseModule).IsAssignableFrom(type))
                            continue;

                        var module = (ISynapseModule)Activator.CreateInstance(type);
                        module.Load();
                        Modules.Add(module);

                        Logger.Get.Info($"Loaded Module {module.Name}");
                    }
                }
                catch(Exception e)
                {
                    Synapse.Api.Logger.Get.Error($"Synapse-Modules: Loading Module failed\n{e}\nPath:\n{file}");
                }
        }
    }
}
