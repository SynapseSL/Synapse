using Synapse.Config;
using System;
using System.Reflection;

namespace Synapse.Api.Plugin.Processors
{
    public class ConfigInjector : IContextProcessor
    {
        public void Process(PluginLoadContext context)
        {
            try
            {
                foreach (var field in context.PluginType.GetFields())
                {
                    var configAttribute = field.GetCustomAttribute<Config>();
                    if (configAttribute == null) continue;
                    var section = configAttribute.section;
                    Type t = FieldInfo.GetFieldFromHandle(field.FieldHandle).FieldType;
                    if (section == null) section = t.FullName?.Replace("."," ");

                    if (!typeof(IConfigSection).IsAssignableFrom(t))
                        continue;

                    object typeObj = Activator.CreateInstance(t);
                    object config = SynapseController.Server.Configs.GetOrSetDefault(section, typeObj);
                    field.SetValue(context.Plugin,config);
                }
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Injector: Injecting Config failed!!\n{e}");
                throw;
            }
        }
    }
}