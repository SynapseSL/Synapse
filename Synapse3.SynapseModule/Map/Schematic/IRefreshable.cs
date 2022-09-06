namespace Synapse3.SynapseModule.Map.Schematic;

public interface IRefreshable
{
    /// <summary>
    /// Updates the Objects
    /// </summary>
    public void Refresh();

    /// <summary>
    /// If the Object should be updated regularly
    /// </summary>
    public bool Update { get; set; }
    
    /// <summary>
    /// The Frequency of which the Object should be updated use -1 or 0 for every frame
    /// </summary>
    public float UpdateFrequency { get; set; }
}