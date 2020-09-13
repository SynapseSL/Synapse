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
        private List<IContextProcessor> Processors = new List<IContextProcessor> { new ConfigInjector() };

        private List<IPlugin> plugins = new List<IPlugin>();

        public List<PluginInformations> Plugins { get; } = new List<PluginInformations>(); 
        
        internal void ActivatePlugins() 
        {
            var paths = Directory.GetFiles(SynapseController.Server.Files.SharedPluginDirectory, "*.dll").ToList();
            paths.AddRange(Directory.GetFiles(SynapseController.Server.Files.PluginDirectory, "*.dll").ToList());

            var dictionary = new Dictionary<PluginInformations, Type>();
            
            var contexts = new List<PluginLoadContext>();

            foreach(var pluginpath in paths)
            {
                var assembly = Assembly.Load(File.ReadAllBytes(pluginpath));
                foreach(var type in assembly.GetTypes())
                {
                    if (!typeof(IPlugin).IsAssignableFrom(type))
                        continue;

                    var infos = type.GetCustomAttribute<PluginInformations>();

                    if (infos == null)
                    {
                        SynapseController.Server.Logger.Info($"The File {assembly.GetName().Name} has a class which inherit from IPlugin but has no PluginInformations ... Default Values will be added");
                        infos = new PluginInformations();
                    }

                    if (pluginpath.Contains("server-shared"))
                        infos.shared = true;

                    dictionary.Add(infos, type);
                }
            }
        
            foreach (var infoTypePair in dictionary.OrderByDescending(x => x.Key.LoadPriority))
                try
                {
                    SynapseController.Server.Logger.Info($"{infoTypePair.Key.Name} will now be activated!");

                    IPlugin plugin = (IPlugin)Activator.CreateInstance(infoTypePair.Value);
                    plugin.Informations = infoTypePair.Key;
                    plugin.Translation = new Translation(plugin.Informations);
                    plugin.PluginDirectory = SynapseController.Server.Files.GetPluginDirectory(plugin.Informations);


                    contexts.Add(new PluginLoadContext(plugin, infoTypePair.Value, infoTypePair.Key));
                    plugins.Add(plugin);
                    Plugins.Add(infoTypePair.Key);
                }
                catch(Exception e) 
                {
                    SynapseController.Server.Logger.Error($"Synapse-Controller: Activation of {infoTypePair.Value.Assembly.GetName().Name} failed!!\n{e}");
                }

            foreach (var context in contexts) 
                foreach (var processor in Processors) 
                    processor.Process(context);

            LoadPlugins();
        }

        private void LoadPlugins()
        {
            foreach (var plugin in plugins)
                try
                {
                    plugin.Load();
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Loader: {plugin.Informations.Name} Loading failed!!\n{e}");
                    throw;
                }
        }

        internal void ReloadConfigs()
        {
            foreach (var plugin in plugins)
                try
                {
                    plugin.Translation.ReloadTranslations();
                    plugin.ReloadConfigs();
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Loader: {plugin.Informations.Name} Reload Config failed!!\n{e}");
                    throw;
                }
        }
    }

    public class PluginLoadContext
    {
        internal PluginLoadContext(IPlugin plugin,Type type, PluginInformations pluginInformations)
        {
            Plugin = plugin;
            PluginType = type;
            Information = pluginInformations;
        }

        public readonly IPlugin Plugin;
        public readonly Type PluginType;
        public readonly PluginInformations Information;
    }

    public interface IContextProcessor
    {
        void Process(PluginLoadContext context);
    }
}
