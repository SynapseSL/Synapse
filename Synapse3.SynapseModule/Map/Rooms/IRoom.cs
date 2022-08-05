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

    public void TurnOffLights(float duration);
}