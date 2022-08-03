using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map.Schematic;

public interface IJoinUpdate
{
    public bool NeedsJoinUpdate { get; }

    /// <summary>
    /// Updates the Objects
    /// </summary>
    public void Refresh(SynapsePlayer player);
}