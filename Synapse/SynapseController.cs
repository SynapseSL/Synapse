using HarmonyLib;
using Synapse.Api.Plugin;
using Synapse.Command;
using Synapse.RCE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class SynapseController
{
    private static bool IsLoaded = false;

    public static Synapse.Server Server { get; } = new Synapse.Server();

    public static PluginLoader PluginLoader { get; } = new PluginLoader();

    public static Handlers CommandHandlers { get; } = new Handlers();

    internal static Queue<QueueAction> ActionQueue { get; } = new Queue<QueueAction>();

    private static SynapseRceServer _rceHandler;

    public static void Init()
    {
        if (IsLoaded) return;
        ServerConsole.AddLog("Welcome to Synapse! :)", ConsoleColor.Cyan);
        IsLoaded = true;
        new SynapseController();
    }
    internal SynapseController()
    {
        SynapseVersion.Init();
        if (StartupArgs.Args.Any(x => x.Equals("-nosynapse", StringComparison.OrdinalIgnoreCase)))
        {
            Server.Logger.Warn("Server started with -nosynapse argument! Synapse will not be loaded");
            return;
        }

        PatchMethods();
        try
        {
            Server.Configs.Init();
            Server.PermissionHandler.Init();
            Server.RoleManager.Init();
            Server.Schematic.Init();
            CommandHandlers.RegisterSynapseCommands();
            PluginLoader.ActivatePlugins();
            Server.Logger.Refresh();

            if (Server.Configs.SynapseConfiguration.UseLocalRceServer)
            {
                _rceHandler = new SynapseRceServer(IPAddress.Loopback, Server.Configs.SynapseConfiguration.RceServerPort);
                Synapse.Api.Events.EventHandler.Get.Server.UpdateEvent += DequeueInConcurrentUnityContext;
                _rceHandler.Start();
            }
        }
        catch (Exception e)
        {
            Server.Logger.Error($"Error while Initialising Synapse! Please fix the Issue and restart your Server:\n{e}");
            return;
        }

        Server.Logger.Info("Synapse is now ready!");
    }

    private void DequeueInConcurrentUnityContext()
    {
        if (ActionQueue.Count != 0)
        {
            var qAction = ActionQueue.Dequeue();
            try
            {
                qAction.Action.Invoke();
            }
            catch (Exception e)
            {
                qAction.Exception = e.InnerException;
            }
            finally
            {
                qAction.Ran = true;
            }
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
