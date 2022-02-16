using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public interface ISynapseObject
    {
        public Dictionary<string, object> ObjectData { get; }

        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Scale { get; set; }

        //This is only used for Shematics
        public Vector3 OriginalScale { get; }

        public GameObject GameObject { get; }

        public Rigidbody Rigidbody { get; }

        public ObjectType Type { get; }

        public void ApplyPhysics();

        public void Destroy();
    }
}
