using System.Collections.Generic;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Role;

namespace Synapse3.ExamplePlugin;

[Automatic]
[Role(
    Name = "ExampleRole",
    Id = 60
)]
public class ExampleRole : SynapseRole
{
    public override int GetTeamID() => 15;

    public override void SpawnPlayer(bool spawnLite)
    {
        Player.RoleType = RoleType.ClassD;
    }

    public override List<int> GetEnemiesID() => new (){ (int)Team.CDP };

    public override void DeSpawn(DespawnReason reason)
    {
        NeuronLogger.For<ExamplePlugin>().Warn(reason);
    }
}