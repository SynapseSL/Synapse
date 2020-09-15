using System;
using System.Reflection;
using Synapse.Command;

namespace Synapse.Api.Plugin.Processors
{
    public class CommandProcessor : IContextProcessor
    {
        public void Process(PluginLoadContext context)
        {
            foreach (var @class in context.Classes)
            {
                if (!typeof(ISynapseCommand).IsAssignableFrom(@class)) continue;
                var inf = @class.GetCustomAttribute<CommandInformations>();
                if (inf == null) continue;
                var classObject = Activator.CreateInstance(@class);
                Handlers.RegisterCommand(classObject as ISynapseCommand);
            }
        }
    }
}