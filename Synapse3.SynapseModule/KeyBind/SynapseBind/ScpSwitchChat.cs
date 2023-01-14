using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.KeyBind.SynapseBind;

[KeyBind(
    Bind = UnityEngine.KeyCode.V,
    CommandName = "ScpChat",
    CommandDescription = "Changes between scp and proximity chat when you are talking as scp"
    )]
public class ScpSwitchChat : SynapseAbstractKeyBind
{
    readonly SynapseConfigService _config;

    public ScpSwitchChat()
    {
        _config = Synapse.Get<SynapseConfigService>();
    }

    public override void Execute(SynapsePlayer player)
    {
        if (!player.ScpController.CanTalk) return;

        player.ScpController.ProximityChat = !player.ScpController.ProximityChat;

        if (player.ScpController.ProximityChat)
            player.SendHint(player.GetTranslation(_config.Translation).TalkingScpSwitchProximity);
        else
            player.SendHint(player.GetTranslation(_config.Translation).TalkingScpSwitchScp);
    }
}

[KeyBind(
    Bind = UnityEngine.KeyCode.B,
    CommandName = "Test1",
    CommandDescription = "Test"
)]
public class Test : SynapseAbstractKeyBind
{
    public override void Execute(SynapsePlayer player)
    {
        SynapseLogger<KeyBindService>.Warn(GetType().Name);
    }
}

[KeyBind(
    Bind = UnityEngine.KeyCode.B,
    CommandName = "Test2",
    CommandDescription = "Test"
)]
public class Test2 : SynapseAbstractKeyBind
{
    public override void Execute(SynapsePlayer player)
    {
        SynapseLogger<KeyBindService>.Warn(GetType().Name);
    }
}

[KeyBind(
    Bind = UnityEngine.KeyCode.B,
    CommandName = "Test3",
    CommandDescription = "Test"
)]
public class Test3 : SynapseAbstractKeyBind
{
    public override void Execute(SynapsePlayer player)
    {
        SynapseLogger<KeyBindService>.Warn(GetType().Name);
    }
}

[KeyBind(
    Bind = UnityEngine.KeyCode.B,
    CommandName = "Test4",
    CommandDescription = "Test"
)]
public class Test4 : SynapseAbstractKeyBind
{
    public override void Execute(SynapsePlayer player)
    {
        SynapseLogger<KeyBindService>.Warn(GetType().Name);
    }
}
