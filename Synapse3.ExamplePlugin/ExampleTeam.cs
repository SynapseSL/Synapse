using System.Collections.Generic;
using Neuron.Core.Logging;
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
    //You don't have to create a constructor but you can get all services with the constructor and Neuron will create the Team with all required services,configs, translation, etc.
    public ExampleTeam(PlayerService service, ExampleConfig config)
    {
        NeuronLogger.For<ExamplePlugin>()
            .Warn("Loaded Config and PlayerService! " + (config != null) + " " + (service != null));
    }
    
    public override void SpawnPlayers(List<SynapsePlayer> players)
    {
        foreach (var player in players)
        {
            player.RoleID = 60;
        }
    }

    public override int MaxWaveSize => 10;

    //This Means Synapse will wait 5 seconds after it's decided that this Team will be spawned
    public override float RespawnTime => 5f;

    //This method is called when it's decided that this team will be spawned and after 5 Seconds (see above) will the players actually be spawned
    public override void RespawnAnnouncement()
    {
        Synapse.Get<CassieService>().Announce("New Team will be spawned in 5 seconds", CassieSettings.DisplayText);
    }
}