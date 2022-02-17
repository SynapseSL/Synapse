using Mirror;
using Synapse.Api.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public abstract class DefaultSynapseObject : ISynapseObject
    {
        public abstract GameObject GameObject { get; }
        public abstract ObjectType Type { get; }

        public Dictionary<string, object> ObjectData => new Dictionary<string, object>();
        public Vector3 OriginalScale { get; internal set; }

        public virtual Vector3 Position
        {
            get => GameObject.transform.position;
            set => GameObject.transform.position = value;
        }

        public virtual Quaternion Rotation
        {
            get => GameObject.transform.rotation;
            set => GameObject.transform.rotation = value;
        }

        public virtual Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set => GameObject.transform.localScale = value;
        }

        public virtual Rigidbody Rigidbody { get; set; }

        public virtual void ApplyPhysics()
            => Rigidbody = GameObject.AddComponent<Rigidbody>();

        public virtual void Destroy()
        {
            if(Map.Get.SynapseObjects.Contains(this))
                Map.Get.SynapseObjects.Remove(this);

            NetworkServer.Destroy(GameObject);
        }
    }
}
