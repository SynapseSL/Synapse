using System.Collections.Generic;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.CustomRole;

public abstract class SynapseRole : ISynapseRole
{
    private SynapsePlayer _player;

    public SynapsePlayer Player
    {
        get => _player;
        set
        {
            if(_player == value)
                return;

            _player?.RemoveCustomRole(DespawnReason.API);

            _player = value;
        }
    }
    
    public abstract string GetRoleName();
    public abstract int GetRoleID();
    public abstract int GetTeamID();
    public abstract List<int> GetFriendsID();
    public abstract List<int> GetEnemiesID();

    public virtual void TryEscape() { }
    public abstract void SpawnPlayer(bool spawnLite);
    public virtual void DeSpawn(DespawnReason reason) { }
}