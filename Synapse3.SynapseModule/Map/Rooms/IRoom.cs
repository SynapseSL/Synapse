using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public interface IRoom
{
    public Vector3 Position { get; }
    
    public Quaternion Rotation { get; }
    
    public GameObject GameObject { get; }
    
    public string Name { get; }
    
    public int ID { get; }
    
    public int Zone { get; }

    public void TurnOffLights(float duration);
}