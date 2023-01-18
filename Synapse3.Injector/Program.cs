using System;
using System.Threading;
using dnlib.DotNet;

namespace Synapse3.Injector
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var injector = new SynapseInjector();
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