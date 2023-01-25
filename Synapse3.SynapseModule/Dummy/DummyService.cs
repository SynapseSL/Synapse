﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using MEC;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Dummy;

public class DummyService : Service
{
    private RoundEvents _round;
    private PlayerEvents _player;

    internal Transform _dummyParent;
    internal readonly List<SynapseDummy> _dummies = new();

    public DummyService(RoundEvents round, PlayerEvents player)
    {
        _round = round;
        _player = player;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(CreateDummyTransformParent);
        _player.Ban.Subscribe(DeSpawnDummyKick);
        _player.Kick.Subscribe(DeSpawnDummyKick);
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(CreateDummyTransformParent);
        _player.Ban.Unsubscribe(DeSpawnDummyKick);
        _player.Kick.Unsubscribe(DeSpawnDummyKick);
    }

    private void DeSpawnDummyKick(KickEvent ev)
    {
        if (ev.Allow && ev.Player is DummyPlayer dummy)
        {
            dummy.SynapseDummy.Destroy();
        }
    }

    public ReadOnlyCollection<SynapseDummy> Dummies => _dummies.AsReadOnly();

    private void CreateDummyTransformParent(RoundWaitingEvent ev)
        => _dummyParent = new GameObject("DummyParent").transform;
}