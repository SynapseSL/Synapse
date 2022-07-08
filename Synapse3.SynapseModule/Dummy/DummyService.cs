using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Dummy;

public class DummyService : Service
{
    private RoundEvents _round;
    
    internal Transform _dummyParent;
    internal readonly List<SynapseDummy> _dummies = new();

    public DummyService(RoundEvents round)
    {
        _round = round;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(CreateDummyTransformParent);
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(CreateDummyTransformParent);
    }

    public ReadOnlyCollection<SynapseDummy> Dummies => _dummies.AsReadOnly();

    private void CreateDummyTransformParent(RoundWaitingEvent ev)
    {
        _dummyParent = new GameObject("DummyParent").transform;
    }
}