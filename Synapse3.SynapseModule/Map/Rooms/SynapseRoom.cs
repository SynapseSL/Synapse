using MapGeneration;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using Synapse3.SynapseModule.Map.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public class SynapseRoom : IVanillaRoom
{
    internal SynapseRoom(RoomIdentifier identifier, RoomType type)
    {
        Identifier = identifier;
        RoomType = type;
        LightController = Identifier.GetComponentInChildren<FlickerableLightController>();

        foreach (var door in Synapse.Get<MapService>().SynapseDoors)
        {
            //Some spawned door by plugin ave null for Rooms
            if (door.Variant.Rooms == null)
                continue;
            if (door.Variant.Rooms.Contains(identifier))
                _doors.Add(door);
        }

        foreach (var interactable in Scp079InteractableBase.AllInstances)
        {
            if (interactable is not Scp079Camera cam) continue;
            if (interactable.Room != identifier) continue;
            _cameras.Add(new SynapseCamera(cam, this));
        }
    }
    
    public RoomIdentifier Identifier { get; }
    
    public Vector3 Position => GameObject.transform.position;
    public Quaternion Rotation => GameObject.transform.rotation;
    public GameObject GameObject => Identifier.gameObject;
    public FlickerableLightController LightController { get; }

    public string Name => RoomType.ToString();
    public RoomType RoomType { get; }
    public uint Id => (uint)RoomType;
    
    public uint Zone
    {
        get
        {
            switch (Position.y)
            {
                case 0f:
                    return (uint)ZoneType.Lcz;

                case 1000f:
                    return (uint)ZoneType.Surface;

                case -1000f:
                    if (Name.Contains("HCZ"))
                        return (uint)ZoneType.Hcz;

                    return (uint)ZoneType.Entrance;

                case -2000f:
                    return (uint)ZoneType.Pocket;

                default:
                    return (uint)ZoneType.None;
            }
        }
    }

    public ZoneType ZoneType => (ZoneType)Zone;
    
    public void TurnOffLights(float duration)
    {
        LightController.ServerFlickerLights(duration);
    }

    private List<SynapseCamera> _cameras = new();
    public ReadOnlyCollection<SynapseCamera> Cameras => _cameras.AsReadOnly();

    public Color RoomColor
    {
        get => LightController.Network_warheadLightColor;
        set
        {
            LightController.Network_warheadLightColor = value == default ? FlickerableLightController.DefaultWarheadColor : value;
            LightController.Network_warheadLightOverride = value != default;
        }
    }


    private List<SynapseDoor> _doors = new List<SynapseDoor>();
    public ReadOnlyCollection<SynapseDoor> Doors => _doors.AsReadOnly();
}