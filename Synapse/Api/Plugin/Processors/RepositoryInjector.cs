using Synapse.Config;
using System;
using System.Reflection;
using Synapse.Database;

namespace Synapse.Api.Plugin.Processors
{
    public class RepositoryInjector : IContextProcessor
    {
        public void Process(PluginLoadContext context)
        {
            try
            {
                foreach (var field in context.PluginType.GetFields())
                {
                    var attribute = field.GetCustomAttribute<Injected>();
                    if (attribute == null) continue;
                    Type t = FieldInfo.GetFieldFromHandle(field.FieldHandle).FieldType;
                    
                    if (!typeof(IRawRepository).IsAssignableFrom(t))
                        continue;

                    
                    object typeObj = Activator.CreateInstance(t);
                    field.SetValue(context.Plugin,typeObj);
                }
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Injector: Injecting Repository failed!!\n{e}");
                throw;
            }
        }
    }
}