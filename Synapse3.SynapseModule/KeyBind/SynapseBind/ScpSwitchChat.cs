using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.KeyBind.SynapseBind;

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
