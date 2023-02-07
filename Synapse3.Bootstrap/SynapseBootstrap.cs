﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PluginAPI.Core.Attributes;

namespace Synapse3.SynapseVector;


public class SynapseBootstrap
{
    [PluginEntryPoint(
        "SynapseBootstrap",
        "1.0.0",
        "Loads Neuron and therefore also Synapse if it is installed",
        "Dimenzio"
    )]
    public void Execute()
    {
        try
        {
            if (StartupArgs.Args.Any(x => x.Equals("-nosynapse", StringComparison.OrdinalIgnoreCase)))
            {
                Log("Server started with -nosynapse argument! SynapsePlatform will not be loaded");
                return;
            }
            
            Log("Bootstrapping SynapsePlatform via reflections", ConsoleColor.Cyan);
            
            var assemblies = new List<Assembly>();
            var domain = AppDomain.CurrentDomain;
            var currentUri = new Uri(Directory.GetCurrentDirectory());
            
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Synapse", "Managed"), "*.dll"))
            {
                try
                {
                    var fileUri = new Uri(file);
                    var assembly = domain.Load(File.ReadAllBytes(file));
                    assemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    Log("Failed to load Assembly\n" + file + "\n" + ex, ConsoleColor.DarkRed);
                }
            }

            domain.AssemblyResolve += delegate(object sender, ResolveEventArgs eventArgs)
            {
                return assemblies.First(x => x.FullName == eventArgs.Name);
            };

            var coreAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == "Synapse3.Platform");
            var entrypoint = coreAssembly.GetType("Synapse3.Platform.SynapseStandalonePlatform");
            if (entrypoint == null) throw new Exception("Synapse3.Platform.SynapseStandalonePlatform not found");
            var main = entrypoint.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            if (main == null) throw new Exception("Synapse3.Platform.SynapseStandalonePlatform.Main() is null");
            main.Invoke(null, Array.Empty<object>());
        }
        catch (Exception e)
        {
            Log($"Failed to load SynapsePlatform: {e}", ConsoleColor.DarkRed);
        }
    }

    private void Log(string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        ServerConsole.AddLog("[Bootstrap] " + msg, color);
    }
}