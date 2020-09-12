using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Synapse.Api.Plugin.Processors;

namespace Synapse.Api.Plugin
{
    public class PluginLoader
    {
        
        private static List<object> plugins = new List<object>();
        
        private static List<IContextProcessor> Processors = new List<IContextProcessor>{new ConfigInjector(), new ExtensionInjector()};
        
        internal void ActivatePlugins() 
        {
            var paths = Directory.GetFiles(SynapseController.Server.Files.PluginDirectory, "*.dll").ToList();
            paths.AddRange(Directory.GetFiles(SynapseController.Server.Files.SharedPluginDirectory, "*.dll").ToList());

            var dictionary = new Dictionary<PluginInformations, Type>();
            
            var contexts = new List<PluginLoadContext>();

            foreach(var pluginpath in paths)
            {
                var assembly = Assembly.Load(File.ReadAllBytes(pluginpath));
                foreach(var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<PluginInformations>() == null) continue;

                    dictionary.Add(type.GetCustomAttribute<PluginInformations>(), type);
                }
            }
        
            foreach (var pluginInfoType in dictionary.OrderByDescending(x => x.Key.LoadPriority))
                try
                {
                    var doesImplement = typeof(IPlugin).IsAssignableFrom(pluginInfoType.Value);

                    if (!doesImplement)
                    {
                        SynapseController.Server.Logger.Error($"MainClass of {pluginInfoType.Key.Name} doesn't implement IPlugin or AbstractPlugin");
                        continue;
                    }
                    
                    SynapseController.Server.Logger.Info($"{pluginInfoType.Key.Name} will now be activated!");

                    object plugin = null;
                    switch (pluginInfoType.Value.GetConstructors().FirstOrDefault().GetParameters().Length)
                    {
                        case 0:
                            plugin = Activator.CreateInstance(pluginInfoType.Value);
                            break;

                        case 1:
                            plugin = Activator.CreateInstance(pluginInfoType.Value, new object[] { new PluginExtension(pluginInfoType.Key)});
                            break;
                    }
                    contexts.Add(new PluginLoadContext(plugin, pluginInfoType.Value, pluginInfoType.Key));
                    plugins.Add(plugin);
                }
                catch(Exception e) 
                {
                    SynapseController.Server.Logger.Error($"Synapse-Controller: Activation of {pluginInfoType.Value.Assembly.GetName().Name} failed!!\n{e}");
                }

            foreach (var context in contexts) foreach (var processor in Processors) processor.Process(context);
            
            try
            {
                foreach (var plugin in plugins) (plugin as IPlugin)?.Load();
                foreach (var plugin in plugins) (plugin as IPlugin)?.Enable();
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Failed Plugin Startup Logic!!\n{e}");
                throw;
            }
        }
    }

    public class PluginLoadContext
    {
        public PluginLoadContext(object plugin, Type pluginType, PluginInformations information)
        {
            Plugin = plugin;
            PluginType = pluginType;
            Information = information;
        }

        public object Plugin;
        public Type PluginType;
        public PluginInformations Information;
    }

    public interface IContextProcessor
    {
        void Process(PluginLoadContext context);
    }
}
