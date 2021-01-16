using System;
using System.Linq;
using System.Reflection;
using Synapse.Command;

namespace Synapse.Api.Plugin.Processors
{
    public class CommandProcessor : IContextProcessor
    {
        public void Process(PluginLoadContext context)
        {
            foreach (var commandType in context.Classes)
            {
                try
                {
                    if (!typeof(ISynapseCommand).IsAssignableFrom(commandType))
                        continue;
                    var cmdInfoAttribute = commandType.GetCustomAttribute<CommandInformation>();
                    if (cmdInfoAttribute == null)
                        continue;

                    object classObject;
                    ConstructorInfo[] allCtors = commandType.GetConstructors();
                    ConstructorInfo diCtor = allCtors.FirstOrDefault(ctorInfo => ctorInfo.GetParameters()
                        .Any(paramInfo => paramInfo.ParameterType == context.PluginType));

                    if (diCtor != null) //If DI-Ctor is found
                        classObject = Activator.CreateInstance(commandType, args: new object[] { context.Plugin });
                    else                //There is no DI-Ctor
                        classObject = Activator.CreateInstance(commandType);
                    
                    Handlers.RegisterCommand(classObject as ISynapseCommand, true);
                }
                catch (Exception e)
                {
                    Logger.Get.Error($"Error loading command {commandType.Name} from {context.Information.Name}\n{e}");
                }
            }
        }
    }
}