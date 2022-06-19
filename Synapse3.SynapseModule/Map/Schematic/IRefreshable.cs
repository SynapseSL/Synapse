namespace Synapse3.SynapseModule.Map.Schematic;

public interface IRefreshable
{
    /// <summary>
    /// Updates the Objects
    /// </summary>
    public void Refresh();

    /// <summary>
    /// If the Object should be automatically be updated every Frame
    /// </summary>
    public bool UpdateEveryFrame { get; }
}