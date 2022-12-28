using PlayerRoles.PlayableScps.Scp079.Cameras;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Objects;

public class SynapseCamera
{
    
    internal SynapseCamera(Scp079Camera camera079, IRoom room)
    {
        Camera = camera079;
        Room = room;
    }
    
    public Scp079Camera Camera { get; }
    
    public IRoom Room { get; }

    public Vector3 Position
    {
        get => Camera.Position;
        set => Camera.Position = value;
    }

    public float HorizontalRotation
    {
        get => Camera.HorizontalRotation;
        set => Camera.HorizontalRotation = value;
    }

    public float VerticalRotation
    {
        get => Camera.VerticalRotation;
        set => Camera.VerticalRotation = value;
    }

    public string Name => Camera.name;

    public ushort CameraID => Camera.SyncId;

    public bool MainCamera => Camera.IsMain;
}