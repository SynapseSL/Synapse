using LiteNetLib;
using LiteNetLib.Utils;
using Neuron.Core.Events;
using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Events;

public class ServerEvents: Service
{
    private readonly EventManager _eventManager;

    public readonly EventReactor<ReloadEvent> Reload = new();
    public readonly EventReactor<PreAuthenticationEvent> PreAuthentication = new();

    public ServerEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    public override void Enable()
    {
        _eventManager.RegisterEvent(Reload);
        _eventManager.RegisterEvent(PreAuthentication);
    }
}

public class ReloadEvent : IEvent { }

public class PreAuthenticationEvent : IEvent
{
    private readonly ConnectionRequest _request;

    public PreAuthenticationEvent(ConnectionRequest request)
    {
        _request = request;
    }
    
    public string UserId { get; set; }

    public bool Allow { get; set; } = true;

    public bool Rejected { get; private set; }

    public void Reject(string reason)
    {
        var data = new NetDataWriter();
        data.Put((byte)10);
        data.Put(reason);
        _request.RejectForce(data);
        Rejected = true;
    }
}