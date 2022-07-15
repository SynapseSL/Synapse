using System.Collections.Generic;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

public interface ISynapseRole
{
    SynapsePlayer Player { get; set; }
    
    RoleAttribute Attribute { get; set; }

    int GetTeamID();

    List<int> GetFriendsID();

    List<int> GetEnemiesID();

    void TryEscape();

    void SpawnPlayer(bool spawnLite);

    void DeSpawn(DespawnReason reason);
}