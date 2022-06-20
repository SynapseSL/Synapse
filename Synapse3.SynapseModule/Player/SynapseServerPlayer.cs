using Synapse3.SynapseModule.Enums;

namespace Synapse3.SynapseModule.Player;

public class SynapseServerPlayer : SynapsePlayer
{
    /// <inheritdoc cref="SynapsePlayer.IsServer"/>
    public override PlayerType PlayerType => PlayerType.Server;

    public override void Awake()
    {
        Synapse.Get<PlayerService>().Host = this;
    }
}