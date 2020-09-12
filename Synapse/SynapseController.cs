using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;
using Synapse.Api.Plugin;
using Synapse.Config;

public class SynapseController
{
    public static Synapse.Server Server { get; private set; }

    private static bool IsLoaded = false;
    
    private static List<object> plugins = new List<object>();
    public static void Init()
    {
        ServerConsole.AddLog("SynapseController has been invoked", ConsoleColor.Cyan);
        if (IsLoaded) return;
        IsLoaded = true;
        var synapse = new SynapseController();
    }

    internal SynapseController()
    {
        CustomNetworkManager.Modded = true;
        Server = new Synapse.Server();
        
        PatchMethods();
        Server.Configs.Init();
        ActivatePlugins();

        Server.Logger.Info("Synapse is now Ready!");
    }

    private void PatchMethods()
    {
        try
        {
            var instance = HarmonyInstance.Create("Synapse.patches.1");
            instance.PatchAll();
            Server.Logger.Info("Harmony Patching was sucessfully!");
        }
        catch(Exception e)
        {
            Server.Logger.Error($"Harmony Patching throw an Error:\n\n {e}");
        }
    }

    private void ActivatePlugins()
    {
        var paths = Directory.GetFiles(Server.Files.PluginDirectory, "*.dll").ToList();
        paths.AddRange(Directory.GetFiles(Server.Files.SharedPluginDirectory, "*.dll").ToList());

        var dictionary = new Dictionary<PluginInformations, Type>();

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
                    Server.Logger.Error($"MainClass of {pluginInfoType.Key.Name} doesn't implement IPlugin or AbstractPlugin");
                    continue;
                }
                
                Server.Logger.Info($"{pluginInfoType.Key.Name} will now be activated!");

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
                InjectIntoPlugin(plugin, pluginInfoType.Key.Name);
                plugins.Add(plugin);
            }
            catch(Exception e)
            {
                Server.Logger.Error($"Synapse-Controller: Activation of {pluginInfoType.Value.Assembly.GetName().Name} failed!!\n{e}");
            }

        try
        {
            foreach (var plugin in plugins) (plugin as IPlugin)?.Load();
            foreach (var plugin in plugins) (plugin as IPlugin)?.Enable();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }

    private void InjectIntoPlugin(object obj, string pluginName)
    {
        try
        {
            foreach (var field in obj.GetType().GetFields())
            {
                var configAttribute = field.GetCustomAttribute<Config>();
                if (configAttribute == null) continue;
                var section = configAttribute.section;
                if (section == null) section = pluginName;
                Type t = FieldInfo.GetFieldFromHandle(field.FieldHandle).FieldType;
                object typeObj = Activator.CreateInstance(t);
                object config = Server.Configs.GetOrSetDefault(section, typeObj);
                field.SetValue(obj,config);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
