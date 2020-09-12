using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SynapseInjector
{
    public class Loader
    {
        /// <summary>
        /// Entrypoint for Synapse
        /// </summary>
        public static void LoadSystem()
        {
            printBanner();

            var synapse = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Synapse");
            if (!Directory.Exists(synapse)) Directory.CreateDirectory(synapse);
            foreach (var depend in Directory.GetFiles(Path.Combine(synapse, "dependencies")))
                Assembly.LoadFile(depend);

            InvokeAssembly(Path.Combine(synapse, "Synapse.dll"));
        }

        /// <summary>
        /// Print Synapse Banner and Version Information
        /// </summary>
        private static void printBanner()
        {
            ServerConsole.AddLog(
                "\nLoading Synapse...\n" +
                "-----------------------------------------------\n" +
                "  __                             \n" +
                " (_       ._    _.  ._    _   _  \n" +
                " __)  \\/  | |  (_|  |_)  _>  (/_ \n" +
                "      /             |            \n" +
                "\n" +
                "LoaderVersion: " + Assembly.GetExecutingAssembly().GetName().Version + "\n" +
                "RuntimeVersion: " + Assembly.GetExecutingAssembly().ImageRuntimeVersion + "\n" +
                "-----------------------------------------------", ConsoleColor.Yellow);

        }

        /// <summary>
        /// Scan assembly for synapse main class
        /// and invoke the init method 
        /// </summary>
        /// <param name="path">The path of the assembly</param>
        private static void InvokeAssembly(string path)
        {
            try
            {
                Assembly.Load(File.ReadAllBytes(path)).GetTypes()
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