using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Events;

public class SynapseObjectEvents : Service
{
    private readonly EventManager _eventManager;
    
    public readonly EventReactor<LoadObjectEvent> LoadObject = new();
    public readonly EventReactor<UpdateObjectEvent> UpdateObject = new();
    public readonly EventReactor<DestroyObjectEvent> DestroyObject = new();

    public SynapseObjectEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(LoadObject);
        _eventManager.RegisterEvent(UpdateObject);
        _eventManager.RegisterEvent(DestroyObject);
    }
}

public class LoadObjectEvent : IEvent
{
    public ISynapseObject SynapseObject { get; }

    public LoadObjectEvent(ISynapseObject synapseObject)
    {
        SynapseObject = synapseObject;
    }
}

public class UpdateObjectEvent : IEvent
{
    public ISynapseObject SynapseObject { get; }

    public UpdateObjectEvent(ISynapseObject synapseObject)
    {
        SynapseObject = synapseObject;
    }
}

public class DestroyObjectEvent : IEvent
{
    public ISynapseObject SynapseObject { get; }

    public DestroyObjectEvent(ISynapseObject synapseObject)
    {
        SynapseObject = synapseObject;
    }
}