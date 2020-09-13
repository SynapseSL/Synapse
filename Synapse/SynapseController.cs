using System;
using Harmony;
using Synapse.Api.Plugin;
using Synapse.Database;

public class SynapseController
{
    public static Synapse.Server Server { get; private set; }

    public static PluginLoader PluginLoader = new PluginLoader();

    public static DatabaseManager Datatabase = new DatabaseManager();
    
    private static bool IsLoaded = false;
    public static bool EnableDatabase = true;
    
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
        PluginLoader.ActivatePlugins();

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

   
}
