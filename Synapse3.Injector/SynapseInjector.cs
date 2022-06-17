using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using FieldAttributes = dnlib.DotNet.FieldAttributes;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using TypeAttributes = dnlib.DotNet.TypeAttributes;

namespace Synapse3.Injector;

public class SynapseInjector
{
    private readonly bool _writeToDisk;
    private readonly string _outputPath;

    public SynapseInjector(bool writeToDisk = true, string outputPath = null)
    {
        _writeToDisk = writeToDisk;
        _outputPath = outputPath ?? Path.Combine(Environment.CurrentDirectory + "/Delivery");
    }

    public void Start(ModuleDefMD loadModule)
    {
        var sourceModule = ModuleDefMD.Load(Assembly.GetExecutingAssembly().Location);

        var loaderSource = sourceModule.Types.First(t => t.Name == "SynapseVector");
        loaderSource.DeclaringType = null;
        var loader = loaderSource.Methods.First(t => t.Name == "Execute");

        SwapTypes(sourceModule, loadModule, loaderSource);

        InjectLoader(loadModule, loader);
        StoreModule(loadModule);
        Publicise(loadModule);
    }

    private void Publicise(ModuleDef md)
    {
        var types = md.Assembly.ManifestModule.Types.ToList();
        var nested = new List<TypeDef>();
        foreach (var type in types)
        {
            if (!type.IsPublic)
            {
                var isInter = type.IsInterface;
                var isAbstr = type.IsAbstract;

                type.Attributes = type.IsNested ? TypeAttributes.NestedPublic : TypeAttributes.Public;

                if (isInter)
                    type.IsInterface = true;
                if (isAbstr)
                    type.IsAbstract = true;
            }

            if (type.CustomAttributes.Find("System.Runtime.CompilerServices.CompilerGeneratedAttribute") != null)
                continue;
            nested.AddRange(type.NestedTypes.ToList());
        }

        foreach (var def in nested)
        {
            if (def.CustomAttributes.Find("System.Runtime.CompilerServices.CompilerGeneratedAttribute") != null)
                continue;
            def.Attributes = def.IsNested ? TypeAttributes.NestedPublic : TypeAttributes.Public;
        }

        types.AddRange(nested);

        foreach (var def in types.SelectMany(t => t.Methods).Where(m => !m?.IsPublic ?? false))
            def.Access = MethodAttributes.Public;

        foreach (var type in types)
        {
            var events = type.Events.Select(_ => _.Name).ToArray();
            foreach (var field in type.Fields)
            {
                var isEventBackingField = events.Any(_ =>
                    String.Equals(_, field.Name, StringComparison.InvariantCultureIgnoreCase));
                if ((!field?.IsPublic ?? false) && !isEventBackingField)
                    field.Access = FieldAttributes.Public;
            }
        }

        if (_writeToDisk)
        {
            md.Write(Path.Combine(_outputPath, "Assembly-CSharp-Publicized.dll"));
            Console.WriteLine("Wrote Assembly-CSharp-Publicized.dll to Delivery directory");
        }
    }

    private void StoreModule(ModuleDef def)
    {
        if (!_writeToDisk)
            return;

        if (!Directory.Exists(_outputPath))
            _ = Directory.CreateDirectory(_outputPath);

        def.Write(Path.Combine(_outputPath, "Assembly-CSharp.dll"));
        Console.WriteLine("Wrote Assembly-CSharp.dll to Delivery directory");
    }

    private void SwapTypes(ModuleDef a, ModuleDef b, TypeDef type)
    {
        _ = a.Types.Remove(type);
        b.Types.Add(type);
    }

    private void InjectLoader(ModuleDef moduleDef, MethodDef callable)
    {
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