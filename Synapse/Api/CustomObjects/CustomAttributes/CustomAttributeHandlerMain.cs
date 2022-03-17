using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public partial class CustomAttributeHandler
    {
        public List<AttributeHandler> Handlers { get; } = new List<AttributeHandler>();

        public List<Type> DefaultAttriutes { get; } = new List<Type>
        {
            typeof(Breakable),
            typeof(SchematicDoor),
        };

        internal void Init()
        {
            foreach(var type in DefaultAttriutes)
                LoadHandlerFromType(type);

            RegisterEvents();
        }

        public void LoadHandlerFromType(Type type)
        {
            try
            {
                if (!typeof(AttributeHandler).IsAssignableFrom(type)) return;

                var handlerobject = Activator.CreateInstance(type);

                if (!(handlerobject is AttributeHandler handler)) return;
                if (string.IsNullOrWhiteSpace(handler.Name)) return;
                if (Handlers.Any(x => x.Name.ToLower() == handler.Name.ToLower())) return;

                Handlers.Add(handler);
                handler.Init();
            }
            catch (Exception ex)
            {
                Logger.Get.Debug($"Synapse-Objects: Type {type?.Name} could not be loaded as AttributeHandler\n{ex}");
            }
        }
    }
}
