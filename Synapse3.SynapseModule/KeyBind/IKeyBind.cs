using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.KeyBind;

public interface IKeyBind
{
    KeyBindAttribute Attribute { get; set; }

    void Execute(SynapsePlayer player);

    void Load();
}
