using System.Collections.Generic;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Role;

namespace Synapse3.ExamplePlugin;

[Automatic]
[Role(
    Name = "ExampleRole",
    Id = 60,
    TeamId = 15
)]
public class ExampleRole : SynapseRole
{
    public override void SpawnPlayer(bool spawnLite)
    {
        //One Example of SpawnLite would be to set a Players PlayerState since the State itself will set Role/Position/Items/Health and so on.
        //This can be used for something like Jail
        if(spawnLite) return;
        
        Player.RoleType = RoleType.NtfCaptain;
        Player.Inventory.ClearAllItems();
        Player.Inventory.GiveItem(ItemType.Coin);
    }

    public override List<uint> GetFriendsID() => new (){ (uint)Team.SCP };

    public override List<uint> GetEnemiesID() => new (){ (uint)Team.CDP };

    public override void DeSpawn(DespawnReason reason)
    {
        NeuronLogger.For<ExamplePlugin>().Warn(reason);
    }
}