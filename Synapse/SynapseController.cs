using HarmonyLib;
using Synapse.Api.Plugin;
using Synapse.Command;
using System;
using System.Linq;

public class SynapseController
{
    private static bool IsLoaded = false;

    public static Synapse.Server Server { get; }
    public static PluginLoader PluginLoader { get; }
    public static Handlers CommandHandlers { get; }

    static SynapseController()
    {
        Server = new Synapse.Server();
        PluginLoader = new PluginLoader();
        CommandHandlers = new Handlers();
    }
    internal SynapseController()
    {
        SynapseVersion.Init();

        if (StartupArgs.Args.Any(x => x.Equals("-nosynapse", StringComparison.OrdinalIgnoreCase)))
        {
            ServerConsole.AddLog("Server started with -nosynapse argument! Synapse will not be loaded", ConsoleColor.Yellow);
            return;
        }

        PatchMethods();

        TryInit(Server.Configs.Init, "Initialising Configs failed");
        TryInit(Server.Logger.Refresh, "Initialising Logger File failed");
        TryInit(Server.PermissionHandler.Init, "Initialising Permissions failed");
        TryInit(Server.RoleManager.Init, "Initialising Roles failed");
        TryInit(Server.Schematic.Init, "Initialising Schematics failed");
        TryInit(Server.RceHandler.Init, "Initialising RCE failed");
        TryInit(CommandHandlers.RegisterSynapseCommands, "Initialising SynapseCommands failed");
        TryInit(PluginLoader.ActivatePlugins, "Initialising Plugins failed");
        TryInit(Server.Logger.Refresh, "Initialising Logger File failed");
        TryInit(Server.RceHandler.Reload, "Reloading RCE failed. Try updating your dependencies");

        Server.Logger.Info("Synapse is now ready!");
    }

    public static void Init()
    {
        if (IsLoaded)
            return;
        ServerConsole.AddLog("Welcome to Synapse! :)", ConsoleColor.Cyan);
        IsLoaded = true;
        _ = new SynapseController();
    }
    private void TryInit(Action init, string msg)
    {
        try
        {
            init();
        }
        catch (Exception ex)
        {
            Server.Logger.Error("Synapse-Loader: " + msg + "\n" + ex);
        }
    }
    private void PatchMethods()
    {
        try
        {
            var instance = new Harmony("synapse.patches");
            instance.PatchAll();
            Server.Logger.Info("Harmony Patching was sucessfully!");
        }
        catch (Exception e)
        {
            Server.Logger.Error($"Harmony Patching threw an error:\n\n {e}");
        }
    }

    public const int SynapseMajor = SynapseVersion.Major;
    public const int SynapseMinor = SynapseVersion.Minor;
    public const int SynapsePatch = SynapseVersion.Patch;
    public const string BasedGameVersion = SynapseVersion.BasedGameVersion;
}
