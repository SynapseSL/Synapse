using dnlib.DotNet;
using System;
using System.Threading;

namespace Synapse.Injector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var injector = new SynapseInjector(true);
            try
            {
                if (args.Length == 0)
                    return;

                var loadModule = ModuleDefMD.Load(args[0], new ModuleCreationOptions());

                injector.Start(loadModule);
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