using System.Collections.Generic;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

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

    public RoleAttribute Attribute { get; set; }
    
    public virtual void Load() { }
    
    public abstract uint GetTeamID();
    public virtual List<uint> GetFriendsID() => new ();
    public virtual List<uint> GetEnemiesID() => new ();

    public virtual void TryEscape() { }
    public abstract void SpawnPlayer(bool spawnLite);
    public virtual void DeSpawn(DespawnReason reason) { }
}