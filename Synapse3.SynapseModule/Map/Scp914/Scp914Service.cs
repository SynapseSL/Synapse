using System;
using System.Collections.Generic;
using System.Linq;
using Neuron.Core.Meta;
using Scp914;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Scp914;

public class Scp914Service : Service
{
    private readonly RoundEvents _round;
    private readonly Synapse _synapseModule;

    public Scp914Service(RoundEvents round, Synapse synapseModule)
    {
        _round = round;
        _synapseModule = synapseModule;

        foreach (var item in (ItemType[])Enum.GetValues(typeof(ItemType)))
        {
            if (item == ItemType.None) continue;
            
            Synapse914Processors[(uint)item] = new List<ISynapse914Processor>();
        }
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(RoundInit);
        
        while (_synapseModule.ModuleScp914BindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleScp914BindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(RoundInit);
    }

    private void RoundInit(RoundWaitingEvent ev)
    {
        Scp914 = Synapse.GetObject<Scp914Controller>();
        _doors = Scp914._doors.Select(x => x.GetSynapseDoor()).ToArray();
    }
    
    
    public Scp914Controller Scp914 { get; private set; }

    public Vector3 Position => Scp914.gameObject.transform.position;

    public Scp914KnobSetting KnobState
    {
        get => Scp914._knobSetting;
        set => Scp914.Network_knobSetting = value;
    }

    public bool IsActive => Scp914._isUpgrading;

    public Vector3 OutputPosition
    {
        get => Scp914.OutputChamber.position;
        set => Scp914.OutputChamber.transform.position = value;
    }

    public Vector3 InputPosition
    {
        get => Scp914.IntakeChamber.position;
        set => Scp914.IntakeChamber.position = value;
    }

    public Vector3 ChamberSize
    {
        get => Scp914._chamberSize;
        set => Scp914._chamberSize = value;
    }

    private SynapseDoor[] _doors;
    public SynapseDoor[] Doors
    {
        get => _doors;
        set
        {
            _doors = value;
            Scp914._doors = value.Select(x => x.Variant).ToArray();
        }
    }

    public void Activate() => Scp914.ServerInteract(null, 0);

    public Dictionary<uint, List<ISynapse914Processor>> Synapse914Processors { get; set; } = new();

    public List<ISynapse914Processor> GetProcessors(uint id) => Synapse914Processors.TryGetValue(id, out var processor)
        ? processor
        : new List<ISynapse914Processor>();

    internal void LoadBinding(SynapseScp914ProcessorBinding binding)
    {
        var processor = (ISynapse914Processor)Synapse.GetOrCreate(binding.Processor);
        foreach (var id in binding.ReplaceHandlers)
        {
            Synapse914Processors[id].Insert(0, processor);
        }
    }
}