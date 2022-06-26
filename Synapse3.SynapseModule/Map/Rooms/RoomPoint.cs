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

    /// <summary>
    /// Position relative to the room
    /// </summary>
    public SerializedVector3 position = Vector3.zero;

    /// <summary>
    /// Rotation relative to the room
    /// </summary>
    public SerializedVector3 rotation = Vector3.zero;

    /// <summary>
    /// Absolute position of the room
    /// </summary>
    public Vector3 GetMapPosition()
    {
        var roomService = Synapse.Get<RoomService>();
        var room = roomService.GetRoom(roomName);
        if (room != null) return room.GameObject.transform.TransformPoint(position);
        
        NeuronLogger.For<Synapse>().Debug("Couldn't find a Room with Name " + roomName + " for the Room Point");
        return Vector3.zero;
    }

    /// <summary>
    /// Absolute rotation of the room
    /// </summary>
    public Quaternion GetMapRotation()
    {
        var roomService = Synapse.Get<RoomService>();
        var room = roomService.GetRoom(roomName);

        if (room != null) return room.Rotation * (Quaternion)rotation;

        NeuronLogger.For<Synapse>().Debug("Couldn't find a Room with Name " + roomName + " for the Room Point");
        return Quaternion.identity;
    }
}