using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Plugin;

public class ReloadablePlugin : Neuron.Core.Plugins.Plugin
{
    public sealed override void Enable()
    {
        Synapse.Get<ServerEvents>().Reload.Subscribe(Reload);
        FirstSetUp();
        EnablePlugin();
    }

    public virtual void FirstSetUp() { }
    
    public virtual void Reload(ReloadEvent _ = null) { }
    
    public virtual void EnablePlugin() { }
}