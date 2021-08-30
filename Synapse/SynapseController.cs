using System;
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
        new SynapseController();
    }

    internal SynapseController()
    {
        SynapseVersion.Init();

        PatchMethods();
        try
        {
            Server.Configs.Init();
            Server.PermissionHandler.Init();
            Server.RoleManager.Init();
            CommandHandlers.RegisterSynapseCommands();

            PluginLoader.ActivatePlugins();
        }
        catch (Exception e)
        {
            Server.Logger.Error($"Error while Initialising Synapse! Please fix the Issue and restart your Server:\n{e}");
            return;
        }

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
