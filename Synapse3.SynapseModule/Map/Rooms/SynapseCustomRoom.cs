using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public abstract class SynapseCustomRoom : IRoom
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public GameObject GameObject { get; }
    public string Name { get; }
    public int ID { get; }
    public int Zone { get; }
    public void TurnOffLights(float duration)
    {
        throw new System.NotImplementedException();
    }
}