using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;

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
        Server = new Synapse.Server();

        PatchMethods();
        ActivatePlugins();
    }

    private void PatchMethods()
    {
        try
        {
            var instance = HarmonyInstance.Create("Synapse.Patches");
            instance.PatchAll();
        }
        catch(Exception e)
        {
            Server.Logger.Error($"Harmony Patchin throw an Error:\n\n {e}");
        }
    }

    private void ActivatePlugins()
    {
        var paths = Directory.GetFiles(Server.Files.PluginDirectory, "*.dll").ToList();
        paths.AddRange(Directory.GetFiles(Server.Files.SharedPluginDirectory, "*.dll").ToList());

        foreach(var pluginpath in paths)
        {
            var assembly = Assembly.LoadFile(pluginpath);

            
        }
    }
}
