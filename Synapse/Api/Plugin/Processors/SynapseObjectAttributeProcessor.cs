using Synapse.Api.CustomObjects.CustomAttributes;
using System;

namespace Synapse.Api.Plugin.Processors
{
    public class SynapseObjectAttributeProcessor : IContextProcessor
    {
        public void Process(PluginLoadContext context)
        {
            foreach (var type in context.Classes)
            {
                try
                {
                    if (!typeof(AttributeHandler).IsAssignableFrom(type))
                        continue;

                    Server.Get.Schematic.AttributeHandler.LoadHandlerFromType(type);
                }
                catch (Exception e)
                {
                    Logger.Get.Error($"Error loading SynapseObject Attribute handler {type.Name} from {context.Information.Name}\n{e}");
                }
            }
        }
    }
}