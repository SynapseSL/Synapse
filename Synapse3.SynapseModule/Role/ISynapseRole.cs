using System.Collections.Generic;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

public interface ISynapseRole
{
    SynapsePlayer Player { get; set; }
    
    RoleAttribute Attribute { get; set; }

    void Load();

    List<uint> GetFriendsID();

    List<uint> GetEnemiesID();

    void TryEscape();

    void SpawnPlayer(bool spawnLite);

    void DeSpawn(DespawnReason reason);
}