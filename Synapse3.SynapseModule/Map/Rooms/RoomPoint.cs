using System;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Config;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

[Serializable]
public class RoomPoint
{
    public RoomPoint() { }

    public RoomPoint(string name, Vector3 relativePosition, Vector3 relativeRotation)
    {
        roomName = name;
        position = relativePosition;
        rotation = relativeRotation;
    }

    public RoomPoint(Vector3 mapPosition, Quaternion mapRotation) : this(Synapse.Get<RoomService>().GetNearestRoom(mapPosition),mapPosition,mapRotation) { }

    public RoomPoint(IRoom relativeRoom, Vector3 mapPosition, Quaternion mapRotation)
    {
        roomName = relativeRoom.Name;
        position = relativeRoom.GameObject.transform.InverseTransformPoint(mapPosition);
        rotation = Quaternion.Inverse(relativeRoom.GameObject.transform.rotation) * mapRotation;
    }

    public string roomName = "";

    public SerializedVector3 position = Vector3.zero;

    public SerializedVector3 rotation = Vector3.zero;

    public Vector3 GetMapPosition()
    {
        var roomService = Synapse.Get<RoomService>();
        var room = roomService.GetRoom(roomName);
        if (room != null) return room.GameObject.transform.TransformPoint(position);
        
        NeuronLogger.For<Synapse>().Debug("Couldn't find a Room with Name " + roomName + " for the MapPoint");
        return Vector3.zero;
    }

    public Quaternion GetMapRotation()
    {
        var roomService = Synapse.Get<RoomService>();
        var room = roomService.GetRoom(roomName);

        if (room != null) return room.Rotation * (Quaternion)rotation;

        NeuronLogger.For<Synapse>().Debug("Couldn't find a Room with Name " + roomName + " for the MapPoint");
        return Quaternion.identity;
    }
}