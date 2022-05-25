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
                    if (configAttribute is null)
                        continue;
                    var section = configAttribute.section;
                    var type = FieldInfo.GetFieldFromHandle(field.FieldHandle).FieldType;
                    if (section is null)
                        section = type.FullName?.Replace(".", " ");

                    if (!typeof(IConfigSection).IsAssignableFrom(type))
                        continue;

                    var typeObj = Activator.CreateInstance(type);
                    var config = SynapseController.Server.Configs.GetOrSetDefault(section, typeObj);
                    field.SetValue(context.Plugin, config);
                }

                foreach (var property in context.PluginType.GetProperties())
                {
                    var configAttribute = property.GetCustomAttribute<Config>();
                    if (configAttribute is null)
                        continue;
                    var section = configAttribute.section;
                    var type = property.PropertyType;
                    if (section is null)
                        section = type.FullName?.Replace(".", " ");

                    if (!typeof(IConfigSection).IsAssignableFrom(type))
                        continue;

                    var typeObj = Activator.CreateInstance(type);
                    var config = SynapseController.Server.Configs.GetOrSetDefault(section, typeObj);
                    property.SetValue(context.Plugin, config);
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