using Neuron.Modules.Configs.Localization;
using Ninject;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Plugin;

public class ReloadableTranslationPlugin<TTranslation> : ReloadablePlugin
    where TTranslation : Translations<TTranslation>, new()
{
    [Inject]
    public TTranslation Translation { get; private set; }

    public sealed override void FirstSetUp() => Reload();
    
    public sealed override void Reload(ReloadEvent _ = null)
    {
        Translation = Synapse.Get<TTranslation>();
        OnReload();
    }
    
    public virtual void OnReload() { }
}