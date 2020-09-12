using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using OpCode = System.Reflection.Emit.OpCode;
using TypeAttributes = dnlib.DotNet.TypeAttributes;

namespace SynapseInjector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {

                if (args.Length == 0)
                {
                    return;
                }

                var program = new Program(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(10000);
            }

            Thread.Sleep(2000);
        }

        private Program(string path)
        {
            var sourceModule = ModuleDefMD.Load(Assembly.GetExecutingAssembly().Location);

            var loaderSource = sourceModule.Types.First(t => t.Name == "Loader");
            loaderSource.DeclaringType = null;
            MethodDef loader = loaderSource.Methods.First(t => t.Name == "LoadSystem");

            var loadModule = LoadAssemblyCSharp(path, true);

            SwapTypes(sourceModule, loadModule, loaderSource);

            InjectLoader(loadModule, loader);
            StoreModule(loadModule);
        }

        private static ModuleDef LoadAssemblyCSharp(string path, bool initial)
        {
            var loadModule = ModuleDefMD.Load(path, new ModuleCreationOptions());
            if (!initial) return loadModule;

            loadModule.Context = ModuleDef.CreateModuleContext();
            var resolver = (AssemblyResolver)loadModule.Context.AssemblyResolver;
            resolver.AddToCache(loadModule);
            return loadModule;
        }

        private static void StoreModule(ModuleDef def)
        {
            var options = new ModuleWriterOptions(def) { MetadataOptions = { Flags = MetadataFlags.KeepOldMaxStack } };
            def.Write("../Assembly-CSharp.dll", options);
            Console.WriteLine("Wrote Assembly-CSharp.dll to parent directory");
        }

        private static void SwapTypes(ModuleDef a, ModuleDef b, TypeDef type)
        {
            a.Types.Remove(type);
            b.Types.Add(type);

        }

        private static void InjectLoader(ModuleDef moduleDef, MethodDef callable)
        {
            MethodDef startMethod = null;

            foreach (var module in moduleDef.Assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (type.Name == "ServerConsole")
                    {
                        startMethod = type.Methods.First(t => t.Name == "Start");
                    }
                }
            }

            startMethod.Body.Instructions.Insert(0, OpCodes.Call.ToInstruction(callable));
        }

    }
}