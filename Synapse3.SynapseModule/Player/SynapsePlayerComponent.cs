using System;
using System.Linq;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Role;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public virtual void Awake()
    {
        if(_player.Players.Contains(this)) return;
        
        _player.AddPlayer(this);
    }

    public virtual void OnDestroy()
    {
        if(!_player.Players.Contains(this)) return;

        _player.RemovePlayer(this);
        
        RemoveCustomRole(DeSpawnReason.Leave);
        _playerEvents.Leave.RaiseSafely(new LeaveEvent(this));
    }

    private float _updateTime;
    public void Update()
    {
        _playerEvents.Update.Raise(new UpdateEvent(this));

        if (PlayerType != PlayerType.Player || HideRank ||
            !string.Equals(SynapseGroup.Color, "rainbow", StringComparison.OrdinalIgnoreCase)) return;

        if (Time.time < _updateTime) return;
        _updateTime = Time.time + _config.PermissionConfiguration.RainbowUpdateTime;
        RankColor = _server.ValidatedBadgeColors.ElementAt(Random.Range(0, _server.ValidatedBadgeColors.Count));
    }
}