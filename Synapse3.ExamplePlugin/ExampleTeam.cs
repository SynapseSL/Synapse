using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Teams;

namespace Synapse3.ExamplePlugin;

[Automatic]
[Team(
    Name = "Example",
    Id = 15
)]
public class ExampleTeam : SynapseTeam
{
    public override void SpawnPlayers(List<SynapsePlayer> players)
    {
        Logger.Warn("Injected: " + (NeuronLoggerInjected != null));
        foreach (var player in players)
        {
            player.RoleID = 60;
        }
    }

    public override int MaxWaveSize => 10;

    //This Means Synapse will wait 5 seconds after it's decided that this Team will be spawned
    public override float RespawnTime => 5f;

    //This method is called when it's decided that this team will be spawned and after 5 Seconds (see above) will the players actually be spawned
    //This will also be called if it is not possible to Spawn the Team since there aren't enough Spectators
    public override void RespawnAnnouncement()
    {
        Synapse.Get<CassieService>().Announce("New Team will be spawned in 5 seconds", CassieSettings.DisplayText);
    }
}