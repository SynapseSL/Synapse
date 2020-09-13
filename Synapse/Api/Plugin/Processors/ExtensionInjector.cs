using System;
using System.Reflection;

namespace Synapse.Api.Plugin.Processors
{
    /*
    public class ExtensionInjector : IContextProcessor

    {
        public void Process(PluginLoadContext context)
        {
            try
            {
                foreach (var field in context.PluginType.GetFields())
                {
                    Type t = FieldInfo.GetFieldFromHandle(field.FieldHandle).FieldType;
                    if (t != typeof(PluginExtension)) continue;
                    var extension = new PluginExtension(context.Information);
                    field.SetValue(context.Plugin,extension);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    */
}