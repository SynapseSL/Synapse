using Neuron.Core.Plugins;
using Neuron.Modules.Configs.Localization;
using Syml;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule;

public class ReloadablePlugin : Plugin
{
    public sealed override void Enable()
    {
        Synapse.Get<ServerEvents>().Reload.Subscribe(Reload);
        FirstSetUp();
        EnablePlugin();
    }

    public virtual void FirstSetUp() => Reload();
    
    public virtual void Reload(ReloadEvent _ = null) { }
    
    public virtual void EnablePlugin() { }
}

public class ReloadablePlugin<TConfig,TTranslation> : ReloadablePlugin
    where TConfig : IDocumentSection
    where TTranslation : Translations<TTranslation>, new()
{
    public TConfig Config { get; private set; }
    public TTranslation Translation { get; private set; }

    public sealed override void FirstSetUp()
    {
        Config = Synapse.Get<TConfig>();
        Translation = Synapse.Get<TTranslation>();
        OnFirstSetUp();
    }
    
    public sealed override void Reload(ReloadEvent _ = null)
    {
        Config = Synapse.Get<TConfig>();
        Translation = Synapse.Get<TTranslation>();
        OnReload();
    }
    
    public virtual void OnReload() { }
    
    public virtual void OnFirstSetUp() { }
}