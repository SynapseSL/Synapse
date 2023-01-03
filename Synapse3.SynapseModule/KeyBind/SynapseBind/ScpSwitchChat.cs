using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.KeyBind.SynapseBind;

[Automatic]
[KeyBind(
    Bind = UnityEngine.KeyCode.V,
    CommandName = "ScpChat",
    CommandDescription = "Changes between scp and proximity chat when you are talking as scp"
    )]
public class ScpSwitchChat : SynapseAbstractKeyBind
{
    public override void Execute(SynapsePlayer player)
    {
        if (!player.ScpController.CanTalk) return;

        player.ScpController.ProximityChat = !player.ScpController.ProximityChat;
    }
}
