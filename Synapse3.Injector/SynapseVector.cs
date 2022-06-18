﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Synapse3.Injector;

/// <summary>
/// Vector that will be injected into the Assembly-CSharp.dll
/// </summary>
public class SynapseVector
{
    public static void Execute()
    {
        try
        {
            ServerConsole.AddLog("Bootstrapping Synapse3 via reflections", ConsoleColor.Cyan);
            var assemblies = new List<Assembly>();
            var domain = AppDomain.CurrentDomain;
            var currentUri = new Uri(Directory.GetCurrentDirectory());
            foreach (var file in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Synapse", "Managed"), "*.dll"))
            {
                var fileUri = new Uri(file);
                var targetUri = currentUri.MakeRelativeUri(fileUri);
                ServerConsole.AddLog($"[Bootstrapp] Loading assembly at {Uri.UnescapeDataString(targetUri.ToString())}", ConsoleColor.DarkGray);
                var assembly = domain.Load(File.ReadAllBytes(file));
                ServerConsole.AddLog($"[Bootstrapp] Loaded assembly {assembly.FullName}");
                assemblies.Add(assembly);
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
            ServerConsole.AddLog($"Failed to load Synapse3: {e}", ConsoleColor.DarkRed);
        }
    }
}