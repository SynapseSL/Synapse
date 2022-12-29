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

    public bool IsActive
    {
        get => Camera.IsActive;
        set => Camera.IsActive = value;
    }

    public Vector3 Position => Camera.CameraPosition;

    public float VerticalRotation
    {
        get => Camera.VerticalRotation;
        set => Camera.VerticalAxis.TargetValue = value;
    }

    public float HorizontalRotation
    {
        get => Camera.HorizontalRotation;
        set => Camera.HorizontalAxis.TargetValue = value;
    }

    public float Zoom
    {
        get => Camera.RollRotation;
        set => Camera.ZoomAxis.TargetValue = value;
    }

    public ushort SyncId => Camera.SyncId;

     public string Name => Camera.name;

    public ushort CameraID => Camera.SyncId;

    public bool MainCamera => Camera.IsMain;
}