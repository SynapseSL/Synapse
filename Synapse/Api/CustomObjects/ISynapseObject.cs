using Synapse.Api.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public interface ISynapseObject
    {
        public Dictionary<string, object> ObjectData { get; }

        public Dictionary<string, string> CustomAttributes { get; }

        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Scale { get; set; }

        public GameObject GameObject { get; }

        public Rigidbody Rigidbody { get; }

        public ObjectType Type { get; }

        public void RemoveParent();

        public void ApplyPhysics();

        public void Destroy();
    }
}
