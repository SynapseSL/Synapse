using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Synapse.Api.Plugin.Processors;
using Synapse.Command;

namespace Synapse.Api.Plugin
{
    public class PluginLoader
    {
        private readonly List<IContextProcessor> _processors = new List<IContextProcessor> { new ConfigInjector(),new CommandProcessor(),new TranslationInjector()};

        private readonly List<IPlugin> _plugins = new List<IPlugin>();

        private readonly List<PluginLoadContext> _contexts = new List<PluginLoadContext>();

        public readonly List<PluginInformation> Plugins = new List<PluginInformation>(); 
        
        internal void ActivatePlugins() 
        {
            var paths = Directory.GetFiles(SynapseController.Server.Files.SharedPluginDirectory, "*.dll").ToList();
            paths.AddRange(Directory.GetFiles(SynapseController.Server.Files.PluginDirectory, "*.dll").ToList());

            var dictionary = new Dictionary<PluginInformation, KeyValuePair<Type, List<Type>>>();
            
            _contexts.Clear();

            foreach(var pluginpath in paths)
            {
                try
                {
                    var assembly = Assembly.Load(File.ReadAllBytes(pluginpath));
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!typeof(IPlugin).IsAssignableFrom(type))
                            continue;

                        var infos = type.GetCustomAttribute<PluginInformation>();

                        if (infos == null)
                        {
                            SynapseController.Server.Logger.Info($"The File {assembly.GetName().Name} has a class which inherits from IPlugin but has no PluginInformation ... Default Values will be added");
                            infos = new PluginInformation();
                        }

                        if (pluginpath.Contains("server-shared"))
                            infos.shared = true;

                        var allTypes = assembly.GetTypes().ToList();
                        allTypes.Remove(type);
                        dictionary.Add(infos, new KeyValuePair<Type, List<Type>>(type, allTypes));
                        break;
                    }
                }
                catch(Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Controller: Loading Assembly of {pluginpath} failed!!\n{e}");
                }
            }
        
            foreach (var infoTypePair in dictionary.OrderByDescending(x => x.Key.LoadPriority))
            {
                try
                {
                    SynapseController.Server.Logger.Info($"{infoTypePair.Key.Name} will now be activated!");

                    IPlugin plugin = (IPlugin)Activator.CreateInstance(infoTypePair.Value.Key);
                    plugin.Information = infoTypePair.Key;
                    plugin.Translation = new Translation(plugin.Information);
                    plugin.PluginDirectory = SynapseController.Server.Files.GetPluginDirectory(plugin.Information);
                    _contexts.Add(new PluginLoadContext(plugin, infoTypePair.Value.Key, infoTypePair.Key, infoTypePair.Value.Value));
                    _plugins.Add(plugin);
                    Plugins.Add(infoTypePair.Key);
                }
                catch(Exception e) 
                {
                    SynapseController.Server.Logger.Error($"Synapse-Controller: Activation of {infoTypePair.Value.Key.Assembly.GetName().Name} failed!!\n{e}");
                }
            }

            foreach (var context in _contexts) 
                foreach (var processor in _processors) 
                    processor.Process(context);

            LoadPlugins();
            Handlers.FinalizePluginsCommands();
        }

        private void LoadPlugins()
        {
            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.Load();
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Loader: {plugin.Information.Name} Loading failed!!\n{e}");
                }
            }
        }

        internal void ReloadConfigs()
        {
            var injector = new ConfigInjector();
            var translationinjector = new TranslationInjector();
            foreach (var context in _contexts)
            {
                injector.Process(context);
                translationinjector.Process(context);
            }

            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.Translation.ReloadTranslations();
                    plugin.ReloadConfigs();
                }
                catch (Exception e)
                {
                    SynapseController.Server.Logger.Error($"Synapse-Loader: {plugin.Information.Name} Reload Config failed!!\n{e}");
                }
            }
        }
    }

    public class PluginLoadContext
    {
        internal PluginLoadContext(IPlugin plugin, Type type, PluginInformation pluginInformation, List<Type> classes)
        {
            Plugin = plugin;
            PluginType = type;
            Information = pluginInformation;
            Classes = classes;
        }

        public readonly IPlugin Plugin;
        public readonly Type PluginType;
        public readonly List<Type> Classes;
        public readonly PluginInformation Information;
    }

    public interface IContextProcessor
    {
        void Process(PluginLoadContext context);
    }
}
