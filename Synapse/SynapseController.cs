using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;
using Synapse.Api.Plugin;

public class SynapseController
{
    public static Synapse.Server Server { get; private set; }

    private static bool IsLoaded = false;
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
        ActivatePlugins();
        Server.Configs.Init();

        Server.Logger.Info("Synapse is now Ready!");
    }

    private void PatchMethods()
    {
        try
        {
            var instance = HarmonyInstance.Create("Synapse.Patches");
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
            var assembly = Assembly.LoadFile(pluginpath);
            foreach(var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<PluginInformations>() == null) continue;

                dictionary.Add(type.GetCustomAttribute<PluginInformations>(), type);
            }
        }


        foreach (var plugintype in dictionary.OrderBy(x => x.Key.LoadPriority))
            try
            {
                Server.Logger.Info($"Activating now {plugintype.Key.Name}");
                Activator.CreateInstance(plugintype.Value);
            }
            catch(Exception e)
            {
                Server.Logger.Error($"Synapse-Controller: Activation of {plugintype.Value.Assembly.GetName().Name} failed!!\n{e}");
            }
    }
}
