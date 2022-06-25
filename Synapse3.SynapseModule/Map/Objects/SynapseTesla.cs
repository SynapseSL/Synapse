using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseTesla
{
    internal SynapseTesla(TeslaGate gate)
    {
        Gate = gate;
        Room = Synapse.Get<RoomService>().GetNearestRoom(gate.transform.position);
    }
    
    public TeslaGate Gate { get; }
    
    public IRoom Room { get; }

    public GameObject GameObject => Gate.gameObject;

    public Vector3 Position => Gate.transform.position;

    public void Trigger() => Gate.RpcPlayAnimation();

    public void InstantTrigger() => Gate.UserCode_RpcInstantBurst();

    public float TriggerSize
    {
        get => Gate.sizeOfTrigger;
        set => Gate.sizeOfTrigger = value;
    }

    public Vector3 KillSize
    {
        get => Gate.sizeOfKiller;
        set => Gate.sizeOfKiller = value;
    }
}