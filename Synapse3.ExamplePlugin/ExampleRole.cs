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
    public override uint GetTeamID() => 15;

    public override void SpawnPlayer(bool spawnLite)
    {
        if(spawnLite) return;
        Player.RoleType = RoleType.NtfCaptain;
        Player.Inventory.ClearAllItems();
        Player.Inventory.GiveItem(ItemType.Coin);
    }

    public override List<uint> GetEnemiesID() => new (){ (uint)Team.CDP };

    public override void DeSpawn(DespawnReason reason)
    {
        NeuronLogger.For<ExamplePlugin>().Warn(reason);
    }
}