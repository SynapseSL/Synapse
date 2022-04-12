using System;
using UnityEngine;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public class StaticTeleporter : BasicTeleporter
    {
        public override string Name => "StaticTeleporter";

        public override Vector3 GetTeleportPosition(ArraySegment<string> args, ISynapseObject synapseObject)
        {
            if (args.Count == 2 && float.TryParse(args.At(1), out var result)) return new Vector3(result, result, result);
            if(args.Count >= 4 && float.TryParse(args.At(1),out var x) && float.TryParse(args.At(2), out var y) && float.TryParse(args.At(3), out var z)) return new Vector3(x, y, z);

            Logger.Get.Warn($"Synapse-Schematic: Teleporter with Invalid Arguments at {synapseObject.Position}. Player will be send to Position 0");
            return Vector3.zero;
        }
    }
}
