using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.KeyBind.SynapseBind;

[KeyBind(
    Bind = UnityEngine.KeyCode.V,
    CommandName = "ScpChat",
    CommandDescription = "Changes between scp and proximity chat when you are talking as scp"
    )]
public class ScpSwitchChat : KeyBind
{
    private readonly SynapseConfigService _config;
    public ScpSwitchChat(SynapseConfigService config) => _config = config;
    
    public override void Execute(SynapsePlayer player)
    {
        if (player.MainScpController.ProximityToggle(out var message, out _))
            player.SendHint(message);
    }
}
