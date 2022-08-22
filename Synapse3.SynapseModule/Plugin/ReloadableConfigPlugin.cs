using Ninject;
using Syml;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Plugin;

public class ReloadableConfigPlugin<TConfig> : ReloadablePlugin
    where TConfig : DocumentSection
{
    [Inject]
    public TConfig Config { get; private set; }

    public sealed override void FirstSetUp() => Reload();

    public sealed override void Reload(ReloadEvent _ = null)
    {
        Config = Synapse.Get<TConfig>();
        OnReload();
    }
    
    public virtual void OnReload() { }
}