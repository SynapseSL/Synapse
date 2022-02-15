using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public interface ISynapseObject
    {
        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Scale { get; set; }

        public GameObject GameObject { get; }

        public Rigidbody Rigidbody { get; }

        public bool IsPrimitive { get; }

        public void ApplyPhysics();
    }
}
