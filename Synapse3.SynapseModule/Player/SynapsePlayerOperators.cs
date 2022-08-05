using Footprinting;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public static implicit operator SynapsePlayer(Footprint footprint) => footprint.Hub.GetSynapsePlayer();
    public static implicit operator SynapsePlayer(ReferenceHub hub) => hub.GetSynapsePlayer();
    public static implicit operator Footprint(SynapsePlayer player) => new(player.Hub);
    public static implicit operator ReferenceHub(SynapsePlayer player) => player.Hub;
    public override string ToString() => NickName;
}