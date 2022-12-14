﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using MapGeneration;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public class SynapseRoom : IVanillaRoom
{
    internal SynapseRoom(RoomIdentifier identifier, RoomType type)
    {
        Identifier = identifier;
        RoomType = type;
        LightController = Identifier.GetComponentInChildren<FlickerableLightController>();

        //TODO:
        /*
        foreach (var camera079 in identifier.GetComponentsInChildren<Camera079>())
        {
            _cameras.Add(new SynapseCamera(camera079, this));
        }
        */
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
}