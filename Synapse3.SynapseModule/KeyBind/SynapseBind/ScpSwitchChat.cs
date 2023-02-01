using PlayerRoles;
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
        if (player.Team != Team.SCPs) return;
        if (!_config.GamePlayConfiguration.SpeakingScp.Contains(player.RoleID) && !player.HasPermission("synapse.scp-proximity")) return;

        player.MainScpController.ProximityChat = !player.MainScpController.ProximityChat;
        var translation = _config.Translation.Get(player);
        player.SendHint(player.MainScpController.ProximityChat
            ? translation.EnableProximity
            : translation.DisableProximity);
    }
}
