using System;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Role;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public virtual void Awake()
    {
        var service = Synapse.Get<PlayerService>();
        if(service.Players.Contains(this)) return;

        service.AddPlayer(this);
    }

    public virtual void OnDestroy()
    {
        var service = Synapse.Get<PlayerService>();
        if(!service.Players.Contains(this)) return;

        service.RemovePlayer(this);
        
        try
        {
            RemoveCustomRole(DespawnReason.Leave);
            var ev = new LeaveEvent(this);
            Synapse.Get<PlayerEvents>().Leave.Raise(ev);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Leave Event failed\n" + ex);
        }
    }

    public void Update()
    {
        Synapse.Get<PlayerEvents>().Update.Raise(new UpdateEvent(this));
    }
}