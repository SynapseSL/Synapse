using System;
using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Map.Schematic.CustomAttributes;

public class CustomAttributeService : Service
{
    private SynapseObjectEvents _events;

    public CustomAttributeService(SynapseObjectEvents events)
    {
        _events = events;
    }
    
    public List<AttributeHandler> Handlers { get; } = new List<AttributeHandler>();
    public List<Type> DefaultAttributes { get; } = new List<Type>
    {
        typeof(SchematicDoor),
        typeof(StaticTeleporter),
        typeof(MapTeleporter),
    };

    public override void Enable()
    {
        foreach(var type in DefaultAttributes)
            LoadHandlerFromType(type);

        RegisterEvents();
    }

    public void LoadHandlerFromType(Type type)
    {
        try
        {
            if (!typeof(AttributeHandler).IsAssignableFrom(type)) return;
            if (type.IsAbstract) return;

            var handlerobject = Activator.CreateInstance(type);

            if (!(handlerobject is AttributeHandler handler)) return;
            if (string.IsNullOrWhiteSpace(handler.Name)) return;
            if (Handlers.Any(x => x.Name.ToLower() == handler.Name.ToLower())) return;

            Handlers.Add(handler);
            handler.Init();
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error($"Sy3 Objects: Type {type?.Name} could not be loaded as AttributeHandler\n{ex}");
        }
    }

    private void RegisterEvents()
    {
        _events.LoadObject.Subscribe(OnLoad);
        _events.UpdateObject.Subscribe(OnUpdate);
        _events.DestroyObject.Subscribe(OnDestroy);
    }
    
    private void OnLoad(LoadObjectEvent ev)
    {
        foreach (var handler in Handlers)
        {
            var name = handler.Name;

            foreach(var attribute in ev.SynapseObject.CustomAttributes)
            {
                if (attribute == null) continue;

                var args = attribute.Split(':');
                if (args[0].ToLower() != handler.Name.ToLower()) continue;
                var newArgs = args.Segment(1);

                handler.SynapseObjects.Add(ev.SynapseObject);
                handler.OnLoad(ev.SynapseObject, newArgs);
                return;
            }
        }
    }

    private void OnDestroy(DestroyObjectEvent ev)
    {
        foreach (var handler in Handlers)
        {
            if (handler.SynapseObjects.Contains(ev.SynapseObject))
            {
                handler.OnDestroy(ev.SynapseObject);
                handler.SynapseObjects.Remove(ev.SynapseObject);
            }
        }
    }

    private void OnUpdate(UpdateObjectEvent ev)
    {
        foreach (var handler in Handlers)
        {
            if (handler.SynapseObjects.Contains(ev.SynapseObject))
                handler.OnUpdate(ev.SynapseObject);
        }
    }
}