using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map.Schematic;

public interface IHideable
{
    public void HideFromAll();

    public void ShowAll();
    
    public void HideFromPlayer(SynapsePlayer player);

    public void ShowPlayer(SynapsePlayer player);
}