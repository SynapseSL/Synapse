using Synapse.Config;
using System;
using System.Reflection;

namespace Synapse.Api.Plugin.Processors
{
    public class TranslationInjector : IContextProcessor
    {
        public void Process(PluginLoadContext context)
        {
            try
            {
                foreach (var field in context.PluginType.GetFields())
                {
                    var translationattribute = field.GetCustomAttribute<SynapseTranslation>();
                    if (translationattribute == null) continue;

                    if (!FieldInfo.GetFieldFromHandle(field.FieldHandle).FieldType.Name.Contains("SynapseTranslation")) continue;

                    if(field.GetValue(context.Plugin) == null)
                    {
                        var translation = Activator.CreateInstance(FieldInfo.GetFieldFromHandle(field.FieldHandle).FieldType, new object[] { Server.Get.Files.GetTranslationPath(context.Information.Name) });

                        field.SetValue(context.Plugin, translation);
                    }
                    else
                        field.FieldType.GetMethod("Reload").Invoke(field.GetValue(context.Plugin), new object[] { });
                }

                foreach (var property in context.PluginType.GetProperties())
                {
                    var translationattribute = property.GetCustomAttribute<SynapseTranslation>();
                    if (translationattribute == null) continue;

                    if (property.Name.Contains("SynapseTranslation")) continue;

                    if (property.GetValue(context.Plugin) == null)
                    {
                        var translation = Activator.CreateInstance(property.PropertyType, new object[] { Server.Get.Files.GetTranslationPath(context.Information.Name) });

                        property.SetValue(context.Plugin, translation);
                    }
                    else
                        property.PropertyType.GetMethod("Reload").Invoke(property.GetValue(context.Plugin), new object[] { });
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
