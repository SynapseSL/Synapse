using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

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
            var loader = loaderSource.Methods.First(t => t.Name == "LoadSystem");

            var loadModule = LoadAssemblyCSharp(path);
                
            SwapTypes(sourceModule,loadModule, loaderSource);
                
            InjectLoader(loadModule, loader);
            StoreModule(loadModule);
        }

        private static ModuleDef LoadAssemblyCSharp(string path)
        {
            return ModuleDefMD.Load(path, new ModuleCreationOptions());
        }
        
        private static void StoreModule(ModuleDef def)
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory + "/Delivery")))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory + "/Delivery"));
            def.Write("./Delivery/Assembly-CSharp.dll");
            Console.WriteLine("Wrote Assembly-CSharp.dll to Delivery directory");
        }
        
        private static void SwapTypes(ModuleDef a, ModuleDef b, TypeDef type)
        {
            a.Types.Remove(type);
            b.Types.Add(type);
        }
        
        private static void InjectLoader(ModuleDef moduleDef, MethodDef callable) {
            MethodDef startMethod = null;
            
            foreach (var module in moduleDef.Assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (type.Name == "CustomNetworkManager")
                    {
                        startMethod = type.Methods.First(t => t.Name == "CreateMatch");
                    }
                }
            }
                
            startMethod?.Body.Instructions.Insert(0, OpCodes.Call.ToInstruction(callable));
        }
        
    }
}