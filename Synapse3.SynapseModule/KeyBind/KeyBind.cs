using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.KeyBind;

public abstract class KeyBind : InjectedLoggerBase, IKeyBind
{
    public KeyBindAttribute Attribute { get; set; }

    public abstract void Execute(SynapsePlayer player);

    public virtual void Load() { }

}
