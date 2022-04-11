using System;
using UnityEngine;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public class MapTeleporter : BasicTeleporter
    {
        public override string Name => "MapTeleporter";

        public override Vector3 GetTeleportPosition(ArraySegment<string> args, ISynapseObject synapseObject)
        {
            if (args.Count >= 5 && MapPoint.TryParse($"{args.At(1)}:{args.At(2)}:{args.At(3)}:{args.At(4)}", out var point)) return point.Position;

            Logger.Get.Warn($"Synapse-Schematic: Teleporter with Invalid Arguments at {synapseObject.Position}. Player will be send to Position 0");
            return Vector3.zero;
        }
    }
}
