using System;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SynapseInjector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please drag an Assembly-File onto the Patcher.");
                Thread.Sleep(1000);
                return;
            }
            
            //Load SL's Code as target
            var targetModule = ModuleDefinition.ReadModule(args[0]);

            var type = targetModule.Types.First(t => t.Name == "CustomNetworkManager");
            var method = type.Methods.First(t => t.Name == "CreateMatch");

            var serverConsole = targetModule.Types.First(t => t.Name == "ServerConsole").Methods.First(t => t.Name == "AddLog");

            var ins = method.Body.Instructions.Last(t => t.OpCode == OpCodes.Call);

            var processor = method.Body.GetILProcessor();

            processor.InsertBefore(ins, Instruction.Create(OpCodes.Ldstr, "Sandro Stinkt <3"));
            processor.InsertBefore(ins, Instruction.Create(OpCodes.Call, serverConsole));

            try
            {
                targetModule.Write("Assembly-CSharpPatched.dll");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(20000);
            }

            Thread.Sleep(20000);
        }
    }
}