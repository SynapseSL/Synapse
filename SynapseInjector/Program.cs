using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using FieldAttributes = dnlib.DotNet.FieldAttributes;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
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
                Thread.Sleep(100000000);
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
            Publicise(loadModule);
        }

        private static void Publicise(ModuleDef md)
        {
            var types = md.Assembly.ManifestModule.Types.ToList();
            var nested = new List<TypeDef>();
            foreach (var type in types)
            {
                type.Attributes = type.IsNested ? TypeAttributes.NestedPublic : TypeAttributes.Public;
                if (type.CustomAttributes.Find("System.Runtime.CompilerServices.CompilerGeneratedAttribute") != null) continue;
                nested.AddRange(type.NestedTypes.ToList());
            }
            foreach (var def in nested)
            {
                if (def.CustomAttributes.Find("System.Runtime.CompilerServices.CompilerGeneratedAttribute") != null) continue;
                def.Attributes = def.IsNested ? TypeAttributes.NestedPublic : TypeAttributes.Public;
            }
            types.AddRange(nested);
            foreach (var def in types.SelectMany(t => t.Methods).Where(m => !m?.IsPublic ?? false)) def.Access = MethodAttributes.Public; 
            foreach (var def in types.SelectMany(t => t.Fields).Where(f => !f?.IsPublic ?? false)) def.Access = FieldAttributes.Public;
            md.Write("./Delivery/Assembly-CSharp-Publicized.dll");
            Console.WriteLine("Wrote Assembly-CSharp-Publicized.dll to Delivery directory");
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