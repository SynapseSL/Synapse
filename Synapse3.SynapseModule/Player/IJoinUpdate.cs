namespace Synapse3.SynapseModule.Player;

public interface IJoinUpdate
{
    public bool NeedsJoinUpdate { get; }

    /// <summary>
    /// Updates the Objects
    /// </summary>
    public void UpdatePlayer(SynapsePlayer player);
}