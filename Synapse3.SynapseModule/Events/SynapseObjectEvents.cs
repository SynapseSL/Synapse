using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Events;

public class SynapseObjectEvents : Service
{
    private readonly EventManager _eventManager;
    
    public readonly EventReactor<LoadObjectEvent> Load = new();
    public readonly EventReactor<UpdateObjectEvent> Update = new();
    public readonly EventReactor<DestroyObjectEvent> Destroy = new();

    public SynapseObjectEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Load);
        _eventManager.RegisterEvent(Update);
        _eventManager.RegisterEvent(Destroy);
    }

    public override void Disable()
    {
        _eventManager.UnregisterEvent(Load);
        _eventManager.UnregisterEvent(Update);
        _eventManager.UnregisterEvent(Destroy);
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