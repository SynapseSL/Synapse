﻿using HarmonyLib;
using Synapse.Api.Plugin;
using Synapse.Command;
using System;
using System.Linq;

public class SynapseController
{
    private static bool IsLoaded = false;

    public static Synapse.Server Server { get; } = new Synapse.Server();

    public static PluginLoader PluginLoader { get; } = new PluginLoader();

    public static Handlers CommandHandlers { get; } = new Handlers();

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
        }
        catch (Exception e)
        {
            Server.Logger.Error($"Error while Initialising Synapse! Please fix the Issue and restart your Server:\n{e}");
            return;
        }

        if (SynapseVersion.Debug)
            Server.Logger.Warn("A Debug Build of Synapse was loaded! This version should only be used for testing and not playing as it loads for longer and is less stable.");

        if (BasedGameVersion != GameCore.Version.VersionString)
            Server.Logger.Warn("Synapse-Version: Different game version than expected. Bugs may occur!");

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
