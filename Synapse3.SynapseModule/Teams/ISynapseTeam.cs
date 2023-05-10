using System.Collections.Generic;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Teams;

public interface ISynapseTeam
{
    public TeamAttribute Attribute { get; set; }

    public void SpawnPlayers(List<SynapsePlayer> players);
    
    public int MaxWaveSize { get; }
    
    public float RespawnTime { get; }

    public void EvacuatePlayer(SynapsePlayer player);

    public void RespawnAnnouncement();

    public void Load();
}