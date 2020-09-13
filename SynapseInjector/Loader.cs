using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SynapseInjector
{
    public class Loader
    {
        /// <summary>
        /// Entrypoint for Synapse
        /// </summary>
        public static void LoadSystem()
        {
            var synapse = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Synapse");
            if (!Directory.Exists(synapse)) Directory.CreateDirectory(synapse);
            
            var dependencyAssemblies = new List<Assembly>();
            foreach (var depend in Directory.GetFiles(Path.Combine(synapse, "dependencies")))
            {
                var assembly = Assembly.LoadFile(depend);
                dependencyAssemblies.Add(assembly);
            };

            var synapseAssembly = Assembly.Load(File.ReadAllBytes(Path.Combine(synapse, "Synapse.dll")));
            
            printBanner(synapseAssembly, dependencyAssemblies);
            
            InvokeAssembly(synapseAssembly);
        }

        /// <summary>
        /// Print Synapse Banner and Version Information
        /// </summary>
        private static void printBanner(Assembly syn, List<Assembly> dep)
        {
            ServerConsole.AddLog(
                "\nLoading Synapse...\n" +
                "-------------------===Loader===-------------------\n" +
                "  __                             \n" +
                " (_       ._    _.  ._    _   _  \n" +
                " __)  \\/  | |  (_|  |_)  _>  (/_ \n" +
                "      /             |            \n\n" +
                $"SynapseVersion {syn.GetName().Version}\n" +
                $"LoaderVersion: {Assembly.GetExecutingAssembly().GetName().Version}\n" +
                $"RuntimeVersion: {Assembly.GetExecutingAssembly().ImageRuntimeVersion}\n\n" +
                string.Join("\n", dep.Select(assembly => $"{assembly.GetName().Name}: {assembly.GetName().Version}").ToList()) + "\n" +
                "-------------------===Loader===-------------------", ConsoleColor.Yellow);

        }

        /// <summary>
        /// Scan assembly for synapse main class
        /// and invoke the init method 
        /// </summary>
        /// <param name="path">The path of the assembly</param>
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
                Console.Error.WriteLine(e);
            }
        }
    }
}