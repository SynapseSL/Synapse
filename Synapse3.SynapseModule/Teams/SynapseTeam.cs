using System.Collections.Generic;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Teams;

public abstract class SynapseTeam : ISynapseTeam
{
    public abstract void SpawnPlayers(List<SynapsePlayer> players);

    public abstract int MaxWaveSize { get; }
    public virtual float RespawnTime => 0f;
    
    public virtual void RespawnAnnouncement() { }
    public TeamAttribute Info { get; set; }
}