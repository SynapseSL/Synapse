using Synapse3.SynapseModule.Map.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public interface IRoom
{
    public Vector3 Position { get; }
    
    public Quaternion Rotation { get; }
    
    public GameObject GameObject { get; }
    
    public string Name { get; }
    
    public uint Id { get; }
    
    public uint Zone { get; }

    ReadOnlyCollection<SynapseDoor> Doors { get; }

    public void TurnOffLights(float duration);
}