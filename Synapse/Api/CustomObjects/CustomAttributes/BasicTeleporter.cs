using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public abstract class BasicTeleporter : AttributeHandler
    {
        public Dictionary<ISynapseObject, ArraySegment<string>> Args { get; }
        public Dictionary<ISynapseObject, float> Distance { get; }

        public BasicTeleporter()
        {
            Args = new Dictionary<ISynapseObject, ArraySegment<string>>();
            Distance = new Dictionary<ISynapseObject, float>();
        }

        public override void OnLoad(ISynapseObject synapseObject, ArraySegment<string> args)
        {
            Args[synapseObject] = args;
            Distance[synapseObject] = args.Count > 0 && Single.TryParse(args.At(0), out var distance) ? distance : 1;
        }
        public override void OnDestroy(ISynapseObject synapseObject)
        {
            _ = Args.Remove(synapseObject);
            _ = Distance.Remove(synapseObject);
        }
        public override void OnUpdate(ISynapseObject synapseObject)
        {
            foreach (var player in Server.Get.Players)
            {
                if (Vector3.Distance(player.Position, synapseObject.Position) < Distance[synapseObject])
                    player.Position = GetTeleportPosition(Args[synapseObject], synapseObject);
            }
        }
        public abstract Vector3 GetTeleportPosition(ArraySegment<string> args, ISynapseObject synapseObject);
    }
}
