using System;
using Synapse3.SynapseModule.Events;

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
    }

    public void Update()
    {
        Synapse.Get<PlayerEvents>().Update.Raise(new UpdateEvent(this));
    }
}