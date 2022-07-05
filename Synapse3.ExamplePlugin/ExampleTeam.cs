using System.Collections.Generic;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Teams;

namespace Synapse3.ExamplePlugin;

[Automatic]
[TeamInformation(
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
}