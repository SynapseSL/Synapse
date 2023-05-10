using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Teams;

public abstract class SynapseTeam : InjectedLoggerBase, ISynapseTeam
{
    public abstract void SpawnPlayers(List<SynapsePlayer> players);

    public abstract int MaxWaveSize { get; }
    public virtual float RespawnTime => 15f;

    public virtual void EvacuatePlayer(SynapsePlayer player) { }

    public virtual void RespawnAnnouncement() { }
    public virtual void Load() { }

    public TeamAttribute Attribute { get; set; }
}