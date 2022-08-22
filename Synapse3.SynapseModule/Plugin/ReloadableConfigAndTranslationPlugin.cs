using Neuron.Modules.Configs.Localization;
using Ninject;
using Syml;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Plugin;

public class ReloadablePlugin<TConfig,TTranslation> : ReloadablePlugin
    where TConfig : IDocumentSection
    where TTranslation : Translations<TTranslation>, new()
{
    [Inject]
    public TConfig Config { get; private set; }
    [Inject]
    public TTranslation Translation { get; private set; }

    public sealed override void FirstSetUp() => Reload();
    
    public sealed override void Reload(ReloadEvent _ = null)
    {
        Config = Synapse.Get<TConfig>();
        Translation = Synapse.Get<TTranslation>();
        OnReload();
    }
    
    public virtual void OnReload() { }
}