namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public static implicit operator SynapsePlayer(Footprinting.Footprint footprint) => footprint.Hub.GetPlayer();
    public static implicit operator SynapsePlayer(ReferenceHub hub) => hub.GetPlayer();
    public static implicit operator Footprinting.Footprint(SynapsePlayer player) => new Footprinting.Footprint(player.Hub);
    public static implicit operator ReferenceHub(SynapsePlayer player) => player.Hub;
}