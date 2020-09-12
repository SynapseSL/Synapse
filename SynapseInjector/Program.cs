using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

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

                var loadModule = ModuleDefMD.Load(args[0]);
                var sourceModule = ModuleDefMD.Load(Assembly.GetExecutingAssembly().Location);
                var options = new ModuleWriterOptions(loadModule)
                {
                    MetadataOptions = {Flags = MetadataFlags.KeepOldMaxStack}
                };

                var methodDef = sourceModule.Types.First(t => t.Name == "Loader").Methods
                    .First(t => t.Name == "LoadSystem");

                methodDef.DeclaringType = null;

                var loadType = new TypeDefUser("Synapse", "Loader");

                loadType.Methods.Add(methodDef);

                loadModule.Types.Add(loadType);
                
                var createMatchDef = loadModule.Types.FirstOrDefault(t => t.Name == "CustomNetworkManager")?.Methods.FirstOrDefault(t => t.Name == "CreateMatch");

                createMatchDef?.Body.Instructions.Append(OpCodes.Call.ToInstruction(methodDef));

                loadModule.Write("PatchedFucker.dll", options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(10000);
            }

            Thread.Sleep(2000);
        }
    }
}