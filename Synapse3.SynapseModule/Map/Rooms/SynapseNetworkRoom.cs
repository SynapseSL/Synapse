﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MapGeneration;
using Mirror;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public class SynapseNetworkRoom : NetworkSynapseObject, IVanillaRoom
{
    internal SynapseNetworkRoom(RoomIdentifier identifier, RoomType type)
    {
        Identifier = identifier;
        RoomType = type;
        NetworkIdentity = GetNetworkIdentity(type);
        LightController = Identifier.GetComponentInChildren<FlickerableLightController>();

        foreach (var door in Synapse.Get<MapService>().SynapseDoors)
        {
            if (door.Variant.Rooms.Contains(identifier))
                _doors.Add(door);
        }

        var comp = identifier.gameObject.AddComponent<SynapseObjectScript>();
        comp.Object = this;
        
        foreach (var interactable in Scp079InteractableBase.AllInstances)
        {
            if (interactable is not Scp079Camera cam) continue;
            if (interactable.Room != identifier) continue;
            _cameras.Add(new SynapseCamera(cam, this));
        }
    }

    public RoomIdentifier Identifier { get; }
    public FlickerableLightController LightController { get; }
    public override NetworkIdentity NetworkIdentity { get; }
    public override GameObject GameObject => Identifier.gameObject;
    
    public override ObjectType Type => ObjectType.Room;
    public string Name => RoomType.ToString();
    public uint Id => (uint)RoomType;
    public RoomType RoomType { get; }
    public ZoneType ZoneType => (ZoneType)Zone;
    public uint Zone
    {
        get
        {
            switch (Position.y)
            {
                case 0f:
                    return (int)ZoneType.Lcz;

                case 1000f:
                    return (int)ZoneType.Surface;

                case -1000f:
                    if (Name.Contains("HCZ"))
                        return (int)ZoneType.Hcz;

                    return (int)ZoneType.Entrance;

                case -2000f:
                    return (int)ZoneType.Pocket;

                default:
                    return (int)ZoneType.None;
            }
        }
    }

    public void TurnOffLights(float duration)
    {
        LightController.ServerFlickerLights(duration);
    }

    public override void OnDestroy()
    {
        Synapse.Get<RoomService>()._rooms.Remove(this);
        base.OnDestroy();
    }

    internal static List<NetworkIdentity> _networkIdentities;
    private NetworkIdentity GetNetworkIdentity(RoomType room)
    {
        if (_networkIdentities == null || _networkIdentities.Count == 0)
            _networkIdentities = Synapse.GetObjects<NetworkIdentity>().Where(x => x.name.Contains("All"))
                .ToList();
        switch (room)
        {
            case RoomType.Scp330:
                return _networkIdentities.FirstOrDefault(x => x?.assetId == 3470525145);
            
            case RoomType.TestingRoom:
                return _networkIdentities.FirstOrDefault(x => x?.assetId == 3172166739);

            default: return null;
        }
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