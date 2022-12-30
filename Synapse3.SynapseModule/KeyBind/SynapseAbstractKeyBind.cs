using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Ninject;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.KeyBind;

public abstract class SynapseAbstractKeyBind : IKeyBind
{
    public KeyBindAttribute Attribute { get; set; }

    protected SynapseAbstractKeyBind() => Synapse.Get<IKernel>().Bind(GetType()).ToConstant(this).InSingletonScope();

    public abstract void Execute(SynapsePlayer player);

    public virtual void Load() { }

}
