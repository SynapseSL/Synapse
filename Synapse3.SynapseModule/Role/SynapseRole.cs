﻿using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Role;

public abstract class SynapseRole : InjectedLoggerBase, ISynapseRole
{
    private SynapsePlayer _player;

    public SynapsePlayer Player
    {
        get => _player;
        set
        {
            if(_player == value)
                return;

            _player?.RemoveCustomRole(DeSpawnReason.API);

            _player = value;
        }
    }

    public RoleAttribute Attribute { get; set; }
    
    public virtual void Load() { }
    
    public virtual List<uint> GetFriendsID() => new ();
    public virtual List<uint> GetEnemiesID() => new ();

    public virtual void TryEscape() { }
    public abstract void SpawnPlayer(ISynapseRole previousRole = null, bool spawnLite = false);
    public virtual void DeSpawn(DeSpawnReason reason) { }
}