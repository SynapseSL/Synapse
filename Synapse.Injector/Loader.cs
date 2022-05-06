using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Synapse.Injector
{
    public class Loader
    {
        /// <summary>
        /// Entrypoint for Synapse
        /// </summary>
        public static void LoadSystem()
        {
            try
            {
                var localpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Synapse");
                var synapsepath = Directory.Exists(localpath)
                    ? localpath
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Synapse");

                if (!Directory.Exists(synapsepath)) Directory.CreateDirectory(synapsepath);

                var dependencyAssemblies = new List<Assembly>();
                foreach (var depend in Directory.GetFiles(Path.Combine(synapsepath, "dependencies"), "*.dll"))
                {
                    try
                    {
                        var assembly = Assembly.Load(File.ReadAllBytes(depend));
                        dependencyAssemblies.Add(assembly);
                    }
                    catch (BadImageFormatException e)
                    {
                        ServerConsole.AddLog($"SynapseLoader: Failed to load Assembly \"{depend}\"! \n{e}", ConsoleColor.Red);
                    }
                }

                var synapseAssembly = Assembly.Load(File.ReadAllBytes(Path.Combine(synapsepath, "Synapse.dll")));

                PrintBanner(synapseAssembly, dependencyAssemblies);
                InvokeAssembly(synapseAssembly);
            }
            catch (Exception e)
            {
                ServerConsole.AddLog($"SynapseLoader: Error occured while loading the assemblies. Please check if all required dll are installed. If you can't fix it join our Discord and show us this Error:\n{e}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Print Synapse Banner and Version Information
        /// </summary>
        private static void PrintBanner(Assembly syn, List<Assembly> dep)
        {
            ServerConsole.AddLog(
                "\nLoading Synapse...\n" +
                "-------------------===Loader===-------------------\n" +
                "  __                             \n" +
                " (_       ._    _.  ._    _   _  \n" +
                " __)  \\/  | |  (_|  |_)  _>  (/_ \n" +
                "      /             |            \n\n" +
                $"SynapseVersion {syn.GetName().Version}\n" +
                $"LoaderVersion: 1.0.1.0\n" +
                $"RuntimeVersion: {Assembly.GetExecutingAssembly().ImageRuntimeVersion}\n\n" +
                string.Join("\n", dep.Select(assembly => $"{assembly.GetName().Name}: {assembly.GetName().Version}").ToList()) + "\n" +
                "-------------------===Loader===-------------------", ConsoleColor.Yellow);

        }

        /// <summary>
        /// Scan assembly for synapse main class
        /// and invoke the init method 
        /// </summary>
        /// <param name="assembly">The Assembly Object</param>
        private static void InvokeAssembly(Assembly assembly)
        {
            try
            {
                assembly.GetTypes()
                    .First((Type t) => t.Name == "SynapseController").GetMethods()
                    .First((MethodInfo m) => m.Name == "Init")
                    .Invoke(null, null);
            }
            catch (Exception e)
            {
                ServerConsole.AddLog($"SynapseLoader: Error while Loading Synapse! Please check your synapse and game version. If you can't fix it join our Discord and show us this Error:\n{e}", ConsoleColor.Red);
            }
        }
    }
}