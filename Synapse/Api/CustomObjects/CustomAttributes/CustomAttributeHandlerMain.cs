using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public partial class CustomAttributeHandler
    {
        public List<AttributeHandler> Handlers { get; } = new List<AttributeHandler>();

        public List<Type> DefaultAttributes { get; } = new List<Type>
        {
            typeof(SchematicDoor),
            typeof(StaticTeleporter),
            typeof(MapTeleporter),
        };

        internal void Init()
        {
            foreach (var type in DefaultAttributes)
                LoadHandlerFromType(type);

            RegisterEvents();
        }

        public void LoadHandlerFromType(Type type)
        {
            try
            {
                if (!typeof(AttributeHandler).IsAssignableFrom(type))
                    return;
                if (type.IsAbstract)
                    return;

                var handlerobject = Activator.CreateInstance(type);

                if (!(handlerobject is AttributeHandler handler))
                    return;
                if (String.IsNullOrWhiteSpace(handler.Name))
                    return;
                if (Handlers.Any(x => x.Name.Equals(handler.Name, StringComparison.InvariantCultureIgnoreCase)))
                    return;

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
