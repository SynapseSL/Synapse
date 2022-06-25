using System;
using System.Collections.Generic;
using UnityEngine;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Map.Schematic.CustomAttributes;

public abstract class BasicTeleporter : AttributeHandler
{
    private readonly PlayerService _player;

    public BasicTeleporter()
    {
        _player = Synapse.Get<PlayerService>();
    }
    
    public Dictionary<ISynapseObject, ArraySegment<string>> Args = new ();
    public Dictionary<ISynapseObject, float> Distance = new ();

    public override void OnLoad(ISynapseObject synapseObject, ArraySegment<string> args)
    {
        Args[synapseObject] = args;
        if (args.Count > 0 && float.TryParse(args.At(0), out var distance))
            Distance[synapseObject] = distance;
        else
            Distance[synapseObject] = 1;
    }

    public override void OnDestroy(ISynapseObject synapseObject)
    {
        Args.Remove(synapseObject);
        Distance.Remove(synapseObject);
    }

    public override void OnUpdate(ISynapseObject synapseObject)
    {
        foreach (var player in _player.Players)
            if (Vector3.Distance(player.Position, synapseObject.Position) < Distance[synapseObject])
                player.Position = GetTeleportPosition(Args[synapseObject], synapseObject);
    }

    public abstract Vector3 GetTeleportPosition(ArraySegment<string> args, ISynapseObject synapseObject);
}