using System;
using System.Collections.Generic;
using Neuron.Core.Meta;
using Ninject;
using Scp914;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Scp914;

public class Scp914Service : Service
{
    private IKernel _kernel;
    private RoundEvents _round;
    public Scp914Service(RoundEvents round, IKernel kernel)
    {
        _kernel = kernel;
        _round = round;

        foreach (var item in (ItemType[])Enum.GetValues(typeof(ItemType)))
        {
            Synapse914Processors[(int)item] = Default914Processor.DefaultProcessor;
        }
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(Set914);
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(Set914);
    }

    private void Set914(RoundWaitingEvent ev)
    {
        Scp914 = Synapse.GetObject<Scp914Controller>();
    }
    
    
    public Scp914Controller Scp914 { get; private set; }

    public Vector3 Position => Scp914.gameObject.transform.position;

    public Scp914KnobSetting KnowState
    {
        get => Scp914._knobSetting;
        set => Scp914.Network_knobSetting = value;
    }

    public bool IsActive => Scp914._isUpgrading;

    public Vector3 OutputPosition
    {
        get => Scp914._outputChamber.position;
        set => Scp914._outputChamber.transform.position = value;
    }

    public Vector3 InputPosition
    {
        get => Scp914._intakeChamber.position;
        set => Scp914._intakeChamber.position = value;
    }

    public Vector3 ChamberSize
    {
        get => Scp914._chamberSize;
        set => Scp914._chamberSize = value;
    }

    public void Activate() => Scp914.ServerInteract(null, 0);

    public Dictionary<int, ISynapse914Processor> Synapse914Processors { get; set; } = new();

    public ISynapse914Processor GetProcessor(int id) => Synapse914Processors.TryGetValue(id, out var processor)
        ? processor
        : Default914Processor.DefaultProcessor;

    internal void LoadBinding(SynapseScp914ProcessorBinding binding)
    {
        var processor = (ISynapse914Processor)_kernel.Get(binding.Processor);
        foreach (var id in binding.ReplaceHandlers)
        {
            Synapse914Processors[id] = processor;
        }
    }
}