using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Synapse.RCE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Synapse.RCE
{
    internal class RoslynCompiler
    {
        internal MethodInfo TryCompile(RceRequest request, out RceResponse failResponse)
        {
            failResponse = null;

            string assemblyName = "RCE-" + request.AssemblyName;
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(request.Code));

            List<MetadataReference> references = new();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (assemblies.Any(_ => _.GetName().Name == assemblyName))
            {
                Api.Logger.Get.Warn("RCE attempted to load an already loaded assembly");
                failResponse = RceResponse.GetAssemblyAlreadyLoadedResponse(assemblyName);
                return null;
            }

            AddDependencies();

            Assembly dynamicAssembly = null;
            try
            {
                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.ConsoleApplication, allowUnsafe: true)
                    );

                var ms = new MemoryStream();
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    dynamicAssembly = Assembly.Load(ms.ToArray());
                }
                else
                {
                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                    failResponse = RceResponse.GetFailedBuildResponse(failures);
                    return null;
                }
            }
            catch (Exception e)
            {
                failResponse = RceResponse.GetFailedBuildResponse(e);
                return null;
            }

            return dynamicAssembly.EntryPoint;

            void AddDependencies()
            {
                references.Add(MetadataReference.CreateFromFile(
                Path.Combine(Server.Get.Files.SynapseDirectory, "Synapse.dll")
                ));

                foreach (var assembly in assemblies.Where(_ => !_.IsDynamic))
                    if (!string.IsNullOrWhiteSpace(assembly.Location))
                        references.Add(MetadataReference.CreateFromFile(assembly.Location));

                foreach (var depend in Directory.GetFiles(Server.Get.Files.DependencyDirectory, "*.dll"))
                    references.Add(MetadataReference.CreateFromFile(depend));
            }
        }
    }
}