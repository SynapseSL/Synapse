using System;
using CommandSystem.Commands;
using HarmonyLib;
using Synapse.Api.Plugin;
using Synapse.Command;

public class SynapseController
{
    private static bool IsLoaded = false;

    public static Synapse.Server Server { get; } = new Synapse.Server();

    public static PluginLoader PluginLoader { get; } = new PluginLoader();

    public static Handlers CommandHandlers { get; } = new Handlers();

    public static void Init()
    {
        ServerConsole.AddLog("Welcome to Synapse! :)", ConsoleColor.Cyan);
        if (IsLoaded) return;
        IsLoaded = true;
        var synapse = new SynapseController();
    }

    internal SynapseController()
    {
        CustomNetworkManager.Modded = true;
        BuildInfoCommand.ModDescription = "A heavily modded server software using extensive runtime patching to make development faster and the usage more accessible to end-users";
        
        PatchMethods();
        Server.Configs.Init();
        Server.PermissionHandler.Init();
        Server.RoleManager.Init();
        CommandHandlers.RegisterSynapseCommands();

        PluginLoader.ActivatePlugins();

        Server.Logger.Info("Synapse is now ready!");
    } 
    
    private void PatchMethods()
    {
        try
        {
            var instance = new Harmony("synapse.patches");
            instance.PatchAll();
            Server.Logger.Info("Harmony Patching was sucessfully!");
        }
        catch(Exception e)
        {
            Server.Logger.Error($"Harmony Patching threw an error:\n\n {e}");
        }
    }

    public const int SynapseMajor = 2;
    public const int SynapseMinor = 5;
    public const int SynapsePatch = 0;
    public const string SynapseVersion = "2.5.0";
}
