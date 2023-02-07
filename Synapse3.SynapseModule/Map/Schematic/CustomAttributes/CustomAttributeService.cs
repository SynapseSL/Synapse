﻿using System;
using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Map.Schematic.CustomAttributes;

public class CustomAttributeService : Service
{
    private readonly SynapseObjectEvents _events;
    private readonly Synapse _synapseModule;

    public CustomAttributeService(SynapseObjectEvents events, Synapse synapseModule)
    {
        _events = events;
        _synapseModule = synapseModule;
    }
    
    public List<AttributeHandler> Handlers { get; } = new();
    public List<Type> DefaultAttributes { get; } = new()
    {
        typeof(SchematicDoor),
        typeof(StaticTeleporter),
        typeof(MapTeleporter),
        typeof(ButtonHandler)
    };

    internal void LoadBinding(SynapseCustomObjectAttributeBinding binding) => LoadHandlerFromType(binding.Type);
    
    public override void Enable()
    {
        foreach(var type in DefaultAttributes)
            LoadHandlerFromType(type);
        
        while (_synapseModule.ModuleObjectAttributeBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleObjectAttributeBindingQueue.Dequeue();
            LoadBinding(binding);
        }
        
        _events.Load.Subscribe(OnLoad);
        _events.Update.Subscribe(OnUpdate);
        _events.Destroy.Subscribe(OnDestroy);
    }

    public override void Disable()
    {
        _events.Load.Unsubscribe(OnLoad);
        _events.Update.Unsubscribe(OnUpdate);
        _events.Destroy.Unsubscribe(OnDestroy);
    }

    public void LoadHandlerFromType(Type type)
    {
        try
        {
            if (!typeof(AttributeHandler).IsAssignableFrom(type)) return;
            if (type.IsAbstract) return;

            var handler = (AttributeHandler)Synapse.GetOrCreate(type);
            if (string.IsNullOrWhiteSpace(handler.Name)) return;
            if (Handlers.Any(x => string.Equals(x.Name, handler.Name, StringComparison.CurrentCultureIgnoreCase))) return;

            Handlers.Add(handler);
            handler.Init();
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error($"Sy3 Objects: Type {type?.Name} could not be loaded as AttributeHandler\n{ex}");
        }
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