using System;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Rooms;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic.CustomAttributes;

public class MapTeleporter : BasicTeleporter
{
    public override string Name => "MapTeleporter";

    public override Vector3 GetTeleportPosition(ArraySegment<string> args, ISynapseObject synapseObject)
    {
        if (args.Count >= 5)
        {
            var point = new RoomPoint();
            point.roomName = args.At(1);
            
            if(!int.TryParse(args.At(2),out var x)) return Vector3.zero;
            if(!int.TryParse(args.At(3),out var y)) return Vector3.zero;
            if(!int.TryParse(args.At(4),out var z)) return Vector3.zero;

            point.position = new SerializedVector3(x, y, z);
            return point.GetMapPosition();
        }
        return Vector3.zero;
    }
}