using System.Collections.Generic;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
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
    public override void SpawnPlayer(ISynapseRole previousRole, bool spawnLite)
    {
        //One Example of SpawnLite would be to set a Players PlayerState since the State itself will set Role/Position/Items/Health and so on.
        //This can be used for something like Jail
        if(spawnLite) return;
        
        Player.RoleType = RoleTypeId.NtfCaptain;
        Player.Inventory.ClearAllItems();
        Player.Inventory.GiveItem(ItemType.Coin);
    }

    public override List<uint> GetFriendsID() => new (){ (uint)Team.SCPs };

    public override List<uint> GetEnemiesID() => new (){ (uint)Team.ClassD };

    public override void DeSpawn(DeSpawnReason reason)
    {
        NeuronLogger.For<ExamplePlugin>().Warn(reason);
    }
}